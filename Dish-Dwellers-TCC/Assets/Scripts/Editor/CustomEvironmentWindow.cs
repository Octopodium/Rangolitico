using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
public class CustomEvironmentWindow : LightingWindowEnvironmentSection
{

    public override void OnEnable()
    {
        Debug.Log("Window Open");
    }

    
    public static void ShowWindow(){
        EditorWindow.GetWindow(typeof(CustomEvironmentWindow));
    }

    public override void OnInspectorGUI()
    {
        // The following will be displayed instead of the Environment section in the LightingWindow
        Debug.Log("Window Updated");
        EditorGUILayout.LabelField("My Custom Environment Section !!");
    }
}

