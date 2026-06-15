using UnityEditor;
using UnityEngine;

namespace MudShip.LipSync.Editor
{
    public class MFAProcessorWindow : EditorWindow
    {
        const string kMfaExe = "MS_LipSync.MfaExe";
        const string kFfmpegExe = "MS_LipSync.FfmpegExe";
        const string kDictPath = "MS_LipSync.DictPath";
        const string kAcousticModel = "MS_LipSync.AcousticModel";

        string _audioPath = "";
        string _transcriptPath = "";
        bool _useTranscript;
        string _savePath = "Assets/VowelData.asset";
        string _mfaExe, _ffmpegExe, _dictPath, _acousticModel;

        [MenuItem("Tools/MS LipSync/MFA Processor")]
        public static void Open() => GetWindow<MFAProcessorWindow>("MFA Processor");

        void OnEnable()
        {
            _mfaExe = EditorPrefs.GetString(kMfaExe, "mfa");
            _ffmpegExe = EditorPrefs.GetString(kFfmpegExe, "ffmpeg");
            _dictPath = EditorPrefs.GetString(kDictPath, "");
            _acousticModel = EditorPrefs.GetString(kAcousticModel, "english_mfa");
        }

        void OnDisable()
        {
            EditorPrefs.SetString(kMfaExe, _mfaExe);
            EditorPrefs.SetString(kFfmpegExe, _ffmpegExe);
            EditorPrefs.SetString(kDictPath, _dictPath);
            EditorPrefs.SetString(kAcousticModel, _acousticModel);
        }

        void OnGUI()
        {
            GUILayout.Label("Audio", EditorStyles.boldLabel);
            FileField("File", ref _audioPath, "wav,mp3");
            _useTranscript = EditorGUILayout.Toggle("Use Transcript", _useTranscript);
            if (_useTranscript)
                FileField("Transcript", ref _transcriptPath, "txt,lab");

            GUILayout.Space(8);
            GUILayout.Label("MFA", EditorStyles.boldLabel);
            _mfaExe = EditorGUILayout.TextField("Executable", _mfaExe);
            FileField("Dictionary", ref _dictPath, "txt,dict");
            _acousticModel = EditorGUILayout.TextField("Acoustic Model", _acousticModel);
            _ffmpegExe = EditorGUILayout.TextField("ffmpeg", _ffmpegExe);

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
                    _dictPath,
                    _acousticModel,
                    _mfaExe,
                    _ffmpegExe,
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
