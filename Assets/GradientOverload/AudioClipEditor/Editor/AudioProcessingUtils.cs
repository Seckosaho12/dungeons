using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AudioClipEditor
{
    public static class AudioProcessingUtils
    {
        private static string UneditedClipsFolderPath;

        public static void ApplyModificationsFromSettings(AudioClip clip)
        {
            SaveUneditedClip(clip);
            AudioClip baseClip = clip;
            string uneditedClipPath = GetUneditedClipPath(clip);
            if (File.Exists(uneditedClipPath))
            {
                baseClip = WavUtility.ToAudioClip(File.ReadAllBytes(uneditedClipPath), clip.name);
            }

            int sampleCount = baseClip.samples;
            int channelCount = baseClip.channels;
            float[] originalSamples = new float[sampleCount * channelCount];
            baseClip.GetData(originalSamples, 0);
            int sampleRate = baseClip.frequency;

            string key = GetEditorPrefKeyFromClip(clip);
            float trimStart = EditorPrefs.GetFloat($"{key}_TrimStart", 0);
            float trimEnd = EditorPrefs.GetFloat($"{key}_TrimEnd", 1);
            float fadeInDuration = EditorPrefs.GetFloat($"{key}_FadeIn", 0);
            float fadeOutDuration = EditorPrefs.GetFloat($"{key}_FadeOut", 0);

            float volume = EditorPrefs.GetFloat($"{key}_Volume", 1);
            bool normalize = EditorPrefs.GetInt($"{key}_Normalize", 0) == 1;
            AnimationCurve fadeInCurve = LoadCurve($"{key}_FadeInCurve", AnimationCurve.Linear(0, 0, 1, 1));
            AnimationCurve fadeOutCurve = LoadCurve($"{key}_FadeOutCurve", AnimationCurve.Linear(0, 0, 1, 1));

            float[] modifiedSamples = ApplyTrim(originalSamples, trimStart, trimEnd);
            ApplyFade(modifiedSamples, modifiedSamples.Length, sampleRate, fadeInDuration, fadeOutDuration, fadeInCurve, fadeOutCurve);
            if (normalize) Normalize(modifiedSamples);
            AdjustVolume(modifiedSamples, volume);

            AudioClip modifiedClip = AudioClip.Create(clip.name, modifiedSamples.Length / channelCount, clip.channels, sampleRate, false);
            modifiedClip.SetData(modifiedSamples, 0);
            byte[] wavData = WavUtility.FromAudioClip(modifiedClip);
            File.WriteAllBytes(AssetDatabase.GetAssetPath(clip), wavData);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip));
        }

        public static void SaveCurve(AnimationCurve curve, string key)
        {
            CurveData curveData = new CurveData
            {
                keyframes = curve.keys.Select(k => new KeyframeData(k)).ToArray()
            };

            string json = JsonUtility.ToJson(curveData);
            EditorPrefs.SetString(key, json);
        }

        public static AnimationCurve LoadCurve(string key, AnimationCurve defaultCurve)
        {
            if (!EditorPrefs.HasKey(key)) return defaultCurve;

            string json = EditorPrefs.GetString(key);
            CurveData curveData = JsonUtility.FromJson<CurveData>(json);

            AnimationCurve curve = new AnimationCurve(curveData.keyframes.Select(k => k.ToKeyframe()).ToArray());
            return curve;
        }

        public static float[] ApplyTrim(float[] samples, float trimStart, float trimEnd)
        {
            int startSample = Mathf.FloorToInt(trimStart * samples.Length);
            int endSample = Mathf.FloorToInt(trimEnd * samples.Length);

            int newLength = endSample - startSample;
            float[] modifiedSamples = new float[newLength];
            Array.Copy(samples, startSample, modifiedSamples, 0, newLength);
            return modifiedSamples;
        }

        public static void Normalize(float[] samples)
        {
            float max = samples.Max(Mathf.Abs);
            if (max > 0)
            {
                float multiplier = 1f / max;
                for (int i = 0; i < samples.Length; i++)
                {
                    samples[i] *= multiplier;
                }
            }
        }

        public static void AdjustVolume(float[] samples, float volume)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= volume;
                samples[i] = Mathf.Clamp(samples[i], -1f, 1f); // Prevent overflow
            }
        }

        public static void ApplyFade(float[] samples, int length, int sampleRate, float fadeInDuration, float fadeOutDuration, AnimationCurve fadeInCurve, AnimationCurve fadeOutCurve)
        {
            int fadeInSamples = Mathf.FloorToInt(fadeInDuration * sampleRate);
            int fadeOutSamples = Mathf.FloorToInt(fadeOutDuration * sampleRate);

            for (int i = 0; i < fadeInSamples && i < length; i++)
            {
                float t = (float)i / fadeInSamples;
                samples[i] *= fadeInCurve.Evaluate(t);

            }

            for (int i = 0; i < fadeOutSamples && i < length; i++)
            {
                float t = (float)i / fadeOutSamples;
                samples[length - i - 1] *= fadeOutCurve.Evaluate(t);
            }
        }

        public static void SaveUneditedClip(AudioClip clip)
        {
            string uneditedClipPath = GetUneditedClipPath(clip);
            if (File.Exists(uneditedClipPath)) return;

            byte[] wavData = WavUtility.FromAudioClip(clip);
            File.WriteAllBytes(uneditedClipPath, wavData);
            AssetDatabase.ImportAsset(uneditedClipPath);
        }

        public static string GetUneditedClipPath(AudioClip clip)
        {
            string assetPath = AssetDatabase.GetAssetPath(clip);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            string[] guids = AssetDatabase.FindAssets("t:Script AudioClipEditorAnchor");
            if (guids.Length == 0)
            {
                Debug.LogError("No AudioClipEditorAnchor script found in the project! It is required to determine the folder for unedited clips.");
                return null;
            }

            string scriptPath = AssetDatabase.GUIDToAssetPath(guids[0]); // Get full script path
            string assetFolderPath = Path.GetDirectoryName(scriptPath); // Get its folder

            UneditedClipsFolderPath = Path.Combine(assetFolderPath, "UneditedAudioClips");
            
            if (!AssetDatabase.IsValidFolder(UneditedClipsFolderPath))
            {
                AssetDatabase.CreateFolder(assetFolderPath, "UneditedAudioClips");
            }

            string uneditedClipName = Path.Combine(UneditedClipsFolderPath, $"{guid}.wav");
            return uneditedClipName;
        }

        public static string GetEditorPrefKeyFromClip(AudioClip clip)
        {
            string assetPath = AssetDatabase.GetAssetPath(clip);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            string key = $"WaveformEditor_{guid}";
            return key;
        }

        public static AudioClip GetUneditedClip(AudioClip clip)
        {
            string uneditedClipPath = GetUneditedClipPath(clip);
            if (!File.Exists(uneditedClipPath)) return null;

            byte[] wavData = File.ReadAllBytes(uneditedClipPath);
            AudioClip uneditedClip = WavUtility.ToAudioClip(wavData, clip.name);
            return uneditedClip;
        }
    }

    [Serializable]
    public class CurveData
    {
        public KeyframeData[] keyframes;
    }

    [Serializable]
    public class KeyframeData
    {
        public float time;
        public float value;
        public float inTangent;
        public float outTangent;

        public KeyframeData(Keyframe key)
        {
            time = key.time;
            value = key.value;
            inTangent = key.inTangent;
            outTangent = key.outTangent;
        }

        public Keyframe ToKeyframe()
        {
            return new Keyframe(time, value, inTangent, outTangent);
        }
    }
}
