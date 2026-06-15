using UnityEditor;
using UnityEngine;

namespace MudShip.LipSync.Editor
{
    [CustomEditor(typeof(VowelMorphApplier))]
    public class VowelMorphApplierEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var smrProp = serializedObject.FindProperty("skinnedMeshRenderer");
            EditorGUILayout.PropertyField(smrProp);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("player"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("smoothing"));

            var smr = smrProp.objectReferenceValue as SkinnedMeshRenderer;
            var names = BlendShapeNames(smr);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Vowel Morphs", EditorStyles.boldLabel);
            if (names.Length == 0)
                EditorGUILayout.HelpBox("Assign a Skinned Mesh Renderer with blend shapes to pick morphs by name.", MessageType.Info);

            var list = serializedObject.FindProperty("vowelMorphs");

            for (int i = 0; i < list.arraySize; i++)
            {
                var entry = list.GetArrayElementAtIndex(i);
                var vowelProp = entry.FindPropertyRelative("vowel");
                var targets = entry.FindPropertyRelative("targets");

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(vowelProp, GUIContent.none);
                        if (GUILayout.Button("Remove Vowel", GUILayout.Width(110)))
                        {
                            list.DeleteArrayElementAtIndex(i);
                            break;
                        }
                    }

                    EditorGUI.indentLevel++;
                    for (int j = 0; j < targets.arraySize; j++)
                    {
                        var tProp = targets.GetArrayElementAtIndex(j);
                        var idxProp = tProp.FindPropertyRelative("blendShapeIndex");
                        var wProp = tProp.FindPropertyRelative("weight");

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (names.Length > 0)
                            {
                                int cur = Mathf.Clamp(idxProp.intValue, 0, names.Length - 1);
                                idxProp.intValue = EditorGUILayout.Popup(cur, names);
                            }
                            else
                            {
                                idxProp.intValue = EditorGUILayout.IntField("Index", idxProp.intValue);
                            }

                            wProp.floatValue = EditorGUILayout.Slider(wProp.floatValue, 0f, 1f, GUILayout.Width(170));

                            if (GUILayout.Button("-", GUILayout.Width(22)))
                            {
                                targets.DeleteArrayElementAtIndex(j);
                                break;
                            }
                        }
                    }

                    if (GUILayout.Button("Add Morph"))
                        targets.arraySize++;
                    EditorGUI.indentLevel--;
                }
            }

            if (GUILayout.Button("Add Vowel"))
                list.arraySize++;

            serializedObject.ApplyModifiedProperties();
        }

        static string[] BlendShapeNames(SkinnedMeshRenderer smr)
        {
            if (smr == null || smr.sharedMesh == null) return new string[0];
            var mesh = smr.sharedMesh;
            var names = new string[mesh.blendShapeCount];
            for (int i = 0; i < names.Length; i++)
                names[i] = i + ": " + mesh.GetBlendShapeName(i);
            return names;
        }
    }
}
