using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PonteLevadica))]
public class EditorPonte : Editor
{
    #region properties

    SerializedProperty rotDesejada;
    SerializedProperty duracao;

    bool showRotation = false;

    #endregion


    private void OnEnable() {
        rotDesejada = serializedObject.FindProperty("rotDesejada");
        duracao = serializedObject.FindProperty("duracao");
    }

    public override void OnInspectorGUI(){
        serializedObject.Update();

        PonteLevadica ponte = (PonteLevadica)target;

        EditorGUILayout.PropertyField(duracao);

        if(GUILayout.Button("Definir rotação desejada")){
            ponte.rotDesejada = ponte.transform.rotation;
        }

        showRotation = EditorGUILayout.BeginFoldoutHeaderGroup(showRotation, "Rotação");
        if(showRotation){
            EditorGUILayout.PropertyField(rotDesejada);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();
    }

}
