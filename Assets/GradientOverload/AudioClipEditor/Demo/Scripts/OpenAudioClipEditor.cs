using UnityEditor;
using UnityEngine;

namespace AudioClipEditor
{
    public class OpenAudioClipEditor : MonoBehaviour
    {
        public void OpenWaveformEditor()
        {
            EditorApplication.ExecuteMenuItem("Window/AudioClip Editor/Waveform Editor");
        }
        
        public void OpenBatchProcessClipsEditor()
        {
            EditorApplication.ExecuteMenuItem("Window/AudioClip Editor/Batch Process Clips");
        }
    }
}
