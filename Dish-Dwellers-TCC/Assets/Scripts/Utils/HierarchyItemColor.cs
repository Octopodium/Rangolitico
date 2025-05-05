/*
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
///<sumary>
/// Codigo adaptado do video: https://youtu.be/H8j4CbnVTfQ?si=oilnJdqfuS9lf0yy
///<sumary>
public static class HierarchyItemColor
{
    static HierarchyItemColor()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarcheWindowItemOnGUI;
    }
 
    static void HierarcheWindowItemOnGUI(int instanceID, Rect selectRect)
    {
        var gameobject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if(gameobject != null && gameobject.name.StartsWith("#",System.StringComparison.Ordinal))
        {
            //Descomente se precisar de cores especificas para itens especificos

            string nameText = gameobject.name.Substring(1);
            if(nameText == "heatwave"){
                EditorGUI.DrawRect(selectRect, new Color(0.5f, 0f, 0f, 1f));
            }else if(nameText == "hookline"){
                EditorGUI.DrawRect(selectRect, new Color(0f, 0f, 0.5f, 1f));
            }else{
                EditorGUI.DrawRect(selectRect, Color.black); //cor default
            }
            EditorGUI.DropShadowLabel(selectRect, gameobject.name.Replace("#", "").ToUpperInvariant());
        }
    }
}
*/
