using UnityEditor;
using UnityEngine;

namespace MudShip.LipSync.Editor
{
    public class MFAProcessorWindow : EditorWindow
    {
        const string kModel = "MS_LipSync.Model";

        string _audioPath = "";
        string _transcriptPath = "";
        bool _useTranscript;
        string _savePath = "Assets/VowelData.asset";
        string _model;

        [MenuItem("Tools/MS LipSync/MFA Processor")]
        public static void Open() => GetWindow<MFAProcessorWindow>("MFA Processor");

        void OnEnable() => _model = EditorPrefs.GetString(kModel, "japanese_mfa");
        void OnDisable() => EditorPrefs.SetString(kModel, _model);

        void OnGUI()
        {
            GUILayout.Label("Audio", EditorStyles.boldLabel);
            FileField("File", ref _audioPath, "wav,mp3,flac,ogg,aiff");
            _useTranscript = EditorGUILayout.Toggle("Use Transcript", _useTranscript);
            if (_useTranscript)
                FileField("Transcript", ref _transcriptPath, "txt,lab");

            GUILayout.Space(8);
            GUILayout.Label("MFA", EditorStyles.boldLabel);
            _model = EditorGUILayout.TextField("Model", _model);

            GUILayout.Space(8);
            GUILayout.Label("Output", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                _savePath = EditorGUILayout.TextField("Asset Path", _savePath);
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    var p = EditorUtility.SaveFilePanelInProject("Save VowelData", "VowelData", "asset", "");
                    if (!string.IsNullOrEmpty(p)) _savePath = p;
                }
            }

            GUILayout.Space(12);
            GUI.enabled = !string.IsNullOrEmpty(_audioPath);
            if (GUILayout.Button("Process", GUILayout.Height(32)))
                Process();
            GUI.enabled = true;
        }

        void FileField(string label, ref string path, string extensions)
        {
            using (new GUILayout.HorizontalScope())
            {
                path = EditorGUILayout.TextField(label, path);
                if (GUILayout.Button("...", GUILayout.Width(30)))
                {
                    var p = EditorUtility.OpenFilePanel("Select " + label, "", extensions);
                    if (!string.IsNullOrEmpty(p)) path = p;
                }
            }
        }

        void Process()
        {
            try
            {
                EditorUtility.DisplayProgressBar("MS LipSync", "Running MFA...", 0.5f);
                var result = VowelDataBuilder.Build(
                    _audioPath,
                    _useTranscript ? _transcriptPath : null,
                    _model,
                    _savePath);

                EditorUtility.ClearProgressBar();
                Debug.Log($"[MS LipSync] {_savePath}: {result.frames.Length} frames, {result.totalDuration:F2}s");
                EditorUtility.RevealInFinder(AssetDatabase.GetAssetPath(result));
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogError($"[MS LipSync] {e}");
                EditorUtility.DisplayDialog("MS LipSync Error", e.Message, "OK");
            }
        }
    }
}
