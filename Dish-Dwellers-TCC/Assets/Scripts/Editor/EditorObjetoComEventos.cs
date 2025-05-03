using UnityEditor;
using UnityEngine.Events;
using UnityEngine;

[CustomEditor(typeof(ObjetoComEventos))]
public class EditorObjetoComEventos : Editor
{
    #region Serialized Properties
    SerializedProperty onDestroy;
    SerializedProperty onDisable;
    SerializedProperty onEnable;
    SerializedProperty onStart;

    bool disable,enable, start, destroy = false;

    #endregion

    public void OnEnable(){
        onDestroy = serializedObject.FindProperty("onDestroy");
        onDisable = serializedObject.FindProperty("onDisable");
        onEnable = serializedObject.FindProperty("onEnable");
        onStart = serializedObject.FindProperty("onStart"); 
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        enable = EditorGUILayout.BeginToggleGroup("Usar eventos OnEnable", enable);  
        if(enable){
            EditorGUILayout.PropertyField(onEnable);
        }
        EditorGUILayout.EndToggleGroup();

        disable = EditorGUILayout.BeginToggleGroup("Usar eventos OnDisable", disable);  
        if(disable){
            EditorGUILayout.PropertyField(onDisable);
        }
        EditorGUILayout.EndToggleGroup();

        start = EditorGUILayout.BeginToggleGroup("Usar eventos OnStart", start);  
        if(start){
            EditorGUILayout.PropertyField(onStart);
        }
        EditorGUILayout.EndToggleGroup();

        destroy = EditorGUILayout.BeginToggleGroup("Usar eventos OnDestroy", destroy);  
        if(destroy){
            EditorGUILayout.PropertyField(onDestroy);
        }
        EditorGUILayout.EndToggleGroup();


        serializedObject.ApplyModifiedProperties();
    }
}
