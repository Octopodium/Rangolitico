using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChaoQueMexe))]
public class ChaoQueMexeEditor : Editor {

    #region properties
    SerializedProperty posO;
    SerializedProperty posF;
    #endregion
    ChaoQueMexe chao;

    void OnEnable() {
        chao = target as ChaoQueMexe;
        posO = serializedObject.FindProperty("posO");
        posF = serializedObject.FindProperty("posF");
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        serializedObject.Update();

        if (GUILayout.Button("Definir Posição Inicial")) {
            Undo.RecordObject(chao, "Posição inicial alterada.");
            chao.posO = chao.transform.position;
            EditorUtility.SetDirty(target);
        }

        if (GUILayout.Button("Definir Posição Final")) {
            Undo.RecordObject(chao, "Posição final alterada.");
            chao.posF = chao.transform.position;
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }

    public void OnSceneGUI() {
        Handles.color = Color.green;
        Handles.DrawWireCube(chao.posO, chao.transform.localScale);

        Handles.color = Color.magenta;
        Handles.DrawWireCube(chao.posF, chao.transform.localScale);
    }

}
