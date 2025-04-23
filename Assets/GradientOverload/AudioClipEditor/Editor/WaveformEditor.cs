using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AudioClipEditor
{
    public class WaveformEditor : EditorWindow
    {
        private AudioClip audioClip;
        private AudioClip selectedAudioClip;
        private float[] samples;
        private float[] modifiedSamples;
        private float trimStart = 0;
        private float trimEnd = 1;
        private float fadeInDuration = 0;
        private float fadeOutDuration = 0;
        private AnimationCurve fadeInCurve = AnimationCurve.Linear(0, 0, 1, 1);
        private AnimationCurve fadeOutCurve = AnimationCurve.Linear(0, 0, 1, 1);
        private bool normalize = false;
        private float volume = 1f;
        private int sampleRate;
        private AudioSource audioSource;
        private bool isPlaying = false;
        private float playheadPosition = 0;
        private string uneditedClipPath;
        private bool madeChanges = false;
        private bool wasSelectionChanged;
    
        private double lastChangeTime = 0;
        private bool subscribedToCheckAndApplyChanges;
    
        private Material lineMaterial;
        
        [MenuItem("Assets/AudioClip Editor/Waveform Editor", false, 1000),
         MenuItem("Window/AudioClip Editor/Waveform Editor", false, 1000)]
        private static void ShowWindow()
        {
            WaveformEditor window = GetWindow<WaveformEditor>("Waveform Editor");
            if (Selection.activeObject is AudioClip clip)
            {
                window.SetAudioClip(clip);
            }
        }

        private void OnEnable()
        {
            EditorApplication.update += Update;
            Selection.selectionChanged += OnSelectionChanged;
            if (!audioSource)
            {
                GameObject go = new GameObject("WaveformEditorAudioSource");
                audioSource = go.AddComponent<AudioSource>();
                go.hideFlags = HideFlags.HideAndDontSave;
            }
            OnSelectionChanged();
        }

        private void OnSelectionChanged()
        {
            if (Selection.activeObject is AudioClip clip)
            {
                selectedAudioClip = clip;
                LoadAudioClipData(clip);
                ApplyRealTimeAdjustments();
                PlayModifiedClip();
                wasSelectionChanged = true;
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
            Selection.selectionChanged -= OnSelectionChanged;
            if (audioSource)
            {
                DestroyImmediate(audioSource.gameObject);
            }
            audioSource = null;
            selectedAudioClip = null;
            ClearWaveformData();
            GC.Collect();
        }
        private void ClearWaveformData()
        {
            samples = null;
            modifiedSamples = null;
            if (lineMaterial != null)
            {
                DestroyImmediate(lineMaterial);
            }
        }

        private void Update()
        {
            if (isPlaying && audioSource.clip != null)
            {
                float newPlayheadPosition = audioSource.time / audioSource.clip.length;
                if (Mathf.Abs(newPlayheadPosition - playheadPosition) > 0.001f)
                {
                    playheadPosition = newPlayheadPosition;
                    Repaint();
                }
            
                if (!audioSource.isPlaying || Mathf.Abs(audioSource.time - audioSource.clip.length) < 0.01f)
                {
                    StopClip(); 
                }
            }
        }
        
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            selectedAudioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", selectedAudioClip, typeof(AudioClip), false);
            if (EditorGUI.EndChangeCheck())
            {
                wasSelectionChanged = true;
            }

            if (selectedAudioClip != null)
            {
                if (samples == null || sampleRate != audioClip.frequency || wasSelectionChanged)
                {
                    LoadAudioClipData(selectedAudioClip);
                    ApplyRealTimeAdjustments();
                    PlayModifiedClip();
                }

                if(madeChanges)
                    ApplyRealTimeAdjustments();
            
                bool applyChanges = false;
                if (Event.current != null)
                {
                    if (Event.current.rawType == EventType.MouseUp && madeChanges)
                    {
                        applyChanges = true;
                        madeChanges = false;
                    }
                }
            
                DrawWaveform();
                DrawTrimControls();
                DrawFadeControls();
                DrawVolumeControls();

                
                if (GUILayout.Button("Reset"))
                {
                    ResetClip();
                }
                
                EditorGUILayout.Space();
                DrawPlaybackControls();
                
                if (applyChanges)
                {
                    ApplyChanges();
                    PlayModifiedClip();
                }
                
                if (wasSelectionChanged)
                {
                    wasSelectionChanged = false;
                    Repaint();
                }
            }
        }

        public void SetAudioClip(AudioClip clip)
        {
            selectedAudioClip = clip;
            LoadAudioClipData(clip);
            LoadSettings();
        }
    
        private void LoadAudioClipData(AudioClip selectedAudioClip)
        {
            if (audioClip != null && audioClip != selectedAudioClip)
            {
                audioClip = null;
            }
        
            uneditedClipPath = AudioProcessingUtils.GetUneditedClipPath(selectedAudioClip);
            if (File.Exists(uneditedClipPath))
            {
                // Load the unedited clip from the file
                audioClip = WavUtility.ToAudioClip(File.ReadAllBytes(uneditedClipPath), selectedAudioClip.name);
                LoadSettings();
            }
            else
            {
                audioClip = selectedAudioClip;
                string key = AudioProcessingUtils.GetEditorPrefKeyFromClip(selectedAudioClip);
                // Delete saved settings data in case the user deleted the unedited clip manually
                EditorPrefs.DeleteKey($"{key}_TrimStart");
                EditorPrefs.DeleteKey($"{key}_TrimEnd");
                EditorPrefs.DeleteKey($"{key}_FadeIn");
                EditorPrefs.DeleteKey($"{key}_FadeOut");
                EditorPrefs.DeleteKey($"{key}_Volume");
                EditorPrefs.DeleteKey($"{key}_Normalize");
                EditorPrefs.DeleteKey($"{key}_FadeInCurve");
                EditorPrefs.DeleteKey($"{key}_FadeOutCurve");
                LoadSettings();
            }

            sampleRate = audioClip.frequency;
            int sampleCount = audioClip.samples * audioClip.channels;
            samples = new float[sampleCount];
            audioClip.GetData(samples, 0);

            modifiedSamples = new float[sampleCount];
            Array.Copy(samples, modifiedSamples, sampleCount);
        }

        private void SaveSettings()
        {
            if (audioClip == null) return;

            string key = AudioProcessingUtils.GetEditorPrefKeyFromClip(selectedAudioClip);

            EditorPrefs.SetFloat($"{key}_TrimStart", trimStart);
            EditorPrefs.SetFloat($"{key}_TrimEnd", trimEnd);
            EditorPrefs.SetFloat($"{key}_FadeIn", fadeInDuration);
            EditorPrefs.SetFloat($"{key}_FadeOut", fadeOutDuration);
            EditorPrefs.SetFloat($"{key}_Volume", volume);
            EditorPrefs.SetInt($"{key}_Normalize", normalize ? 1 : 0);

            AudioProcessingUtils.SaveCurve(fadeInCurve, $"{key}_FadeInCurve");
            AudioProcessingUtils.SaveCurve(fadeOutCurve, $"{key}_FadeOutCurve");
        }

        private void LoadSettings()
        { if (audioClip == null) return;

            string key = AudioProcessingUtils.GetEditorPrefKeyFromClip(selectedAudioClip);

            trimStart = EditorPrefs.GetFloat($"{key}_TrimStart",0);
            trimEnd = EditorPrefs.GetFloat($"{key}_TrimEnd",1);
            fadeInDuration = EditorPrefs.GetFloat($"{key}_FadeIn",0);
            fadeOutDuration = EditorPrefs.GetFloat($"{key}_FadeOut",0);
            volume = EditorPrefs.GetFloat($"{key}_Volume",1);
            normalize = EditorPrefs.GetInt($"{key}_Normalize",0) == 1;
        
            string fadeInCurveJson = EditorPrefs.GetString($"{key}_FadeInCurve","");
            if (!string.IsNullOrEmpty(fadeInCurveJson) && fadeInCurveJson != "{}")
            {
                fadeInCurve = AudioProcessingUtils.LoadCurve($"{key}_FadeInCurve", AnimationCurve.Linear(0, 0, 1, 1));
            }
            else
            {
                fadeInCurve = AnimationCurve.Linear(0, 0, 1, 1);
            }

            string fadeOutCurveJson = EditorPrefs.GetString($"{key}_FadeOutCurve","");
            if (!string.IsNullOrEmpty(fadeOutCurveJson) && fadeInCurveJson != "{}")
            {
                fadeOutCurve = AudioProcessingUtils.LoadCurve($"{key}_FadeOutCurve",AnimationCurve.Linear(0, 0, 1, 1));
            }
            else
            {
                fadeOutCurve = AnimationCurve.Linear(0, 0, 1, 1);
            }
        }


        private void ApplyRealTimeAdjustments()
        {
            if (samples == null) return;

            modifiedSamples = AudioProcessingUtils.ApplyTrim(samples, trimStart, trimEnd);
            AudioProcessingUtils.ApplyFade(modifiedSamples, modifiedSamples.Length, sampleRate, fadeInDuration, fadeOutDuration, fadeInCurve, fadeOutCurve);
            if (normalize) AudioProcessingUtils.Normalize(modifiedSamples);
            AudioProcessingUtils.AdjustVolume(modifiedSamples, volume);
        }
        
        void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                lineMaterial.SetInt("_ZWrite", 0);
                lineMaterial.SetFloat("_AntiAliasing", 1.0f); // Smooth lines
            }
        }
        
        private void DrawWaveform()
        {
            if (modifiedSamples == null || modifiedSamples.Length == 0) return;

            EditorGUILayout.LabelField("Waveform Preview", EditorStyles.boldLabel);

            float width = position.width - 20;
            float height = 100;
            Rect rect = GUILayoutUtility.GetRect(width, height);

            if (Event.current.type == EventType.Repaint)
            {
                Color originalColor = Handles.color;
                Handles.color = new Color(0f, 0f, 0f, 0.4f); // Light gray with transparency
                Handles.DrawSolidRectangleWithOutline(new Rect(rect.x, rect.y, rect.width, rect.height), Handles.color, Color.clear);
                Handles.color = originalColor;

                CreateLineMaterial();
                lineMaterial.SetPass(0);
                GL.PushMatrix();
                GL.LoadPixelMatrix();
                GL.Begin(GL.LINES);
                GL.Color(Color.cyan);
                
                // Higher resolution sampling
                int step = Mathf.Max(1, modifiedSamples.Length / (int)(width * 4)); 
                float centerY = rect.y + rect.height / 2;
                float amplitude = rect.height / 2;
            
                for (int i = 0; i < modifiedSamples.Length - step; i += step)
                {
                    float sample = modifiedSamples[i];
                    float nextSample = modifiedSamples[i + step];
                
                    // Ensure samples stay within [-1, 1] before visual scaling
                    sample = Mathf.Clamp(sample, -1f, 1f);
                    nextSample = Mathf.Clamp(nextSample, -1f, 1f);
            
                    // Amplify low volume sections using logarithmic scaling
                    sample = Mathf.Sign(sample) * Mathf.Log10(Mathf.Abs(sample) * 9 + 1);
                    nextSample = Mathf.Sign(nextSample) * Mathf.Log10(Mathf.Abs(nextSample) * 9 + 1);
            
                    float x1 = rect.x + ((float)i / modifiedSamples.Length) * rect.width;
                    float y1 = centerY - sample * amplitude;
                    float x2 = rect.x + ((float)(i + step) / modifiedSamples.Length) * rect.width;
                    float y2 = centerY - nextSample * amplitude;
            
                    //Handles.DrawLine(new Vector3(x1, y1), new Vector3(x2, y2)); less performant
                    GL.Vertex3(x1, y1, 0);
                    GL.Vertex3(x2, y2, 0);
                }
                GL.End();
                GL.PopMatrix();

                // Draw playthrough line if playing
                if (isPlaying)
                {
                    Handles.color = Color.red;
                    float playheadX = rect.x + playheadPosition * rect.width;
                    Handles.DrawLine(new Vector3(playheadX, rect.y), new Vector3(playheadX, rect.y + rect.height));
                }
            }
        }

        private void DrawTrimControls()
        {
            EditorGUILayout.LabelField("Trim", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            float oldTrimStart = trimStart;
            float oldTrimEnd = trimEnd;
        
            float newTrimStart = EditorGUILayout.Slider("Start", trimStart, 0f, 1f);
            float newTrimEnd = EditorGUILayout.Slider("End", trimEnd, 0f, 1f);

            if (EditorGUI.EndChangeCheck())
            {
                trimStart = newTrimStart;
                trimEnd = newTrimEnd;
            }
        
            if(!Mathf.Approximately(oldTrimStart, newTrimStart) || !Mathf.Approximately(oldTrimEnd, newTrimEnd))
            {
                madeChanges = true;
            }
        }

        private void DrawFadeControls()
        {
            float oldFadeInDuration = fadeInDuration;
            float oldFadeOutDuration = fadeOutDuration;
            float originalClipDuration = (float)samples.Length / (audioClip.channels * audioClip.frequency);
            float maxFadeValue = originalClipDuration * (trimEnd - trimStart)*audioClip.channels;
            fadeInDuration = EditorGUILayout.Slider("Fade In Duration", fadeInDuration, 0, maxFadeValue);
            fadeOutDuration = EditorGUILayout.Slider("Fade Out Duration", fadeOutDuration, 0, maxFadeValue);
        
            EditorGUI.BeginChangeCheck();
        
            AnimationCurve newFadeInCurve = EditorGUILayout.CurveField("Fade In Curve", fadeInCurve);
            AnimationCurve newFadeOutCurve = EditorGUILayout.CurveField("Fade Out Curve", fadeOutCurve);

            if (EditorGUI.EndChangeCheck())
            {
                fadeInCurve = newFadeInCurve;
                fadeOutCurve = newFadeOutCurve;

                lastChangeTime = EditorApplication.timeSinceStartup;
                if(subscribedToCheckAndApplyChanges)
                {
                    EditorApplication.update -= CheckAndApplyChanges;
                }
                EditorApplication.update += CheckAndApplyChanges;
                subscribedToCheckAndApplyChanges = true;
                
                madeChanges = true;
            }
        
            if(!Mathf.Approximately(oldFadeInDuration, fadeInDuration) || !Mathf.Approximately(oldFadeOutDuration, fadeOutDuration))
            {
                madeChanges = true;
            }
        }
    
        private void CheckAndApplyChanges()
        {
            if (EditorApplication.timeSinceStartup - lastChangeTime >= 0.5f)
            {
                ApplyChanges();
                PlayModifiedClip();
                Repaint();
            
                EditorApplication.update -= CheckAndApplyChanges;
                subscribedToCheckAndApplyChanges = false;
            }
        }

        private void DrawVolumeControls()
        {
            bool newNormalize = EditorGUILayout.Toggle("Normalize", normalize);
            float newVolume = EditorGUILayout.Slider("Volume", volume, 0f, 2f);
        
            if (!Mathf.Approximately(newVolume, volume))
            {
                volume = newVolume;
                madeChanges = true;
            }
        
            if (newNormalize != normalize)
            {
                normalize = newNormalize;
                ApplyChanges();
                PlayModifiedClip();
            }
        }
    
        private void ApplyChanges()
        {
            if (audioClip == null) return;

            AudioProcessingUtils.SaveUneditedClip(selectedAudioClip);

            ApplyRealTimeAdjustments();
        
            // Create new AudioClip
            AudioClip newClip = AudioClip.Create(selectedAudioClip.name, modifiedSamples.Length / audioClip.channels, audioClip.channels, audioClip.frequency, false);
            newClip.SetData(modifiedSamples, 0);

            // get the path of the AudioClip
            string path = AssetDatabase.GetAssetPath(selectedAudioClip);
            byte[] wavData = WavUtility.FromAudioClip(newClip);
            File.WriteAllBytes(path, wavData);
            AssetDatabase.ImportAsset(path);
            SaveSettings();
        }

        private void ResetClip()
        {
            StopClip();
            uneditedClipPath = AudioProcessingUtils.GetUneditedClipPath(selectedAudioClip);
            if (string.IsNullOrEmpty(uneditedClipPath) || !File.Exists(uneditedClipPath))
                return;
        
            byte[] wavData = File.ReadAllBytes(uneditedClipPath);
            string path = AssetDatabase.GetAssetPath(selectedAudioClip);
            File.WriteAllBytes(AssetDatabase.GetAssetPath(selectedAudioClip), File.ReadAllBytes(uneditedClipPath));
            AssetDatabase.ImportAsset(path);
        
            AssetDatabase.DeleteAsset(uneditedClipPath);
            uneditedClipPath = null;

            audioClip = selectedAudioClip;
        
            trimStart = 0;
            trimEnd = 1;
            fadeInDuration = 0;
            fadeOutDuration = 0;
            fadeInCurve = AnimationCurve.Linear(0, 0, 1, 1);
            fadeOutCurve = AnimationCurve.Linear(0, 0, 1, 1);
            normalize = false;
            volume = 1f;

            string key = AudioProcessingUtils.GetEditorPrefKeyFromClip(selectedAudioClip);
        
            EditorPrefs.DeleteKey($"{key}_TrimStart");
            EditorPrefs.DeleteKey($"{key}_TrimEnd");
            EditorPrefs.DeleteKey($"{key}_FadeIn");
            EditorPrefs.DeleteKey($"{key}_FadeOut");
            EditorPrefs.DeleteKey($"{key}_Volume");
            EditorPrefs.DeleteKey($"{key}_Normalize");
            EditorPrefs.DeleteKey($"{key}_FadeInCurve");
            EditorPrefs.DeleteKey($"{key}_FadeOutCurve");
        
            modifiedSamples = new float[wavData.Length];
            Array.Copy(wavData, modifiedSamples, wavData.Length);
            ApplyRealTimeAdjustments();
            Repaint();
        }
    
        private void DrawPlaybackControls()
        {
            if (EditorUtility.audioMasterMute)
            {
                EditorGUILayout.HelpBox("Audio is muted in Game View. Unmute to hear playback!", MessageType.Warning);
            }
            EditorGUILayout.BeginHorizontal();
            if (isPlaying)
            {
                if (GUILayout.Button("⏹️ Stop"))
                {
                    StopClip();
                }
            }
            else
            {
                if (GUILayout.Button("▶️ Play"))
                {
                    PlayModifiedClip();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void PlayModifiedClip()
        {
            AudioClip clip = AudioClip.Create("ModifiedClip", modifiedSamples.Length/audioClip.channels, audioClip.channels, sampleRate, false);
            clip.SetData(modifiedSamples, 0);
            audioSource.clip = clip;
            audioSource.Play();
            isPlaying = true;
        }

        private void StopClip()
        {
            audioSource.Stop();
            isPlaying = false;
            playheadPosition = 0;
        }
    }
}
