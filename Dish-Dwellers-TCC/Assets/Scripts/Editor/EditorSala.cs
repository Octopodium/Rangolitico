using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(sala))]
public class EditorSala : Editor
{
    #region Serialized Properties
    SerializedProperty resetaveis;
    #endregion

    private void OnEnable(){
        resetaveis = serializedObject.FindProperty("resetaveis");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        sala sala = target as sala;

        serializedObject.Update();

        if(GUILayout.Button("Econtrar resetaveis")){
            sala.resetaveis = FindObjectsByType<IResetavel>(FindObjectsSortMode.None).ToList();
            foreach(var data in sala.resetaveis){
                Debug.Log($"<color=yellow>{data.name}");
            }
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }

}
