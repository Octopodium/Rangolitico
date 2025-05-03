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
        duracao = serializedObject.FindProperty("duracao");
        rotDesejada = serializedObject.FindProperty("rotDesejada");
    }

    public override void OnInspectorGUI(){
        serializedObject.Update();
        PonteLevadica ponte = target as PonteLevadica;

        EditorGUILayout.PropertyField(duracao);

        if(GUILayout.Button("Definir rotação desejada")){
            ponte.rotDesejada = ponte.transform.rotation;
        }

        showRotation = EditorGUILayout.BeginFoldoutHeaderGroup(showRotation, "Rotação");
        if(showRotation){
            EditorGUILayout.PropertyField(rotDesejada);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorUtility.SetDirty(target);
        
        serializedObject.ApplyModifiedProperties();
    }

}
