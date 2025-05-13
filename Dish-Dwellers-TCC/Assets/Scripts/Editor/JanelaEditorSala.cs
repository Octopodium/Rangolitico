using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class JanelaEditorSala : EditorWindow
{
    [Header("Environment Settings Asset")]
    [SerializeField] Dictionary<string, EnvironmentSettingsAsset> table = new Dictionary<string, EnvironmentSettingsAsset>();
    [SerializeField] EnvironmentSettingsAsset data;
    [SerializeField] EnvironmentSettingsAsset[] environmentSettingByLevel;
    private readonly string path = "Assets/EvironmentSettings/Data.asset";

    [Header("Configuration")]
    [SerializeField] AmbientMode ambientMode;
    [SerializeField] Color ambientColor;
    [SerializeField] Material skyboxMat;

    [SerializeField] Scene currentScene;


    private void OnEnable(){
        //SetDataByScene();
        EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
    }

    [MenuItem("Window/JanelaEditorSala")]
    public static void ShowWindow(){
        GetWindow(typeof(JanelaEditorSala));
    }


    private void OnSceneChanged(Scene previous, Scene next){
        currentScene = next;

        if(table.ContainsKey(next.name)){
            data = table[next.name];
            SetConfigurationFromAsset();
        }
        else{
            SetDataByScene(next);
        }
    }

    private void SetDataByScene(Scene scene){
        data = null;
        GetConfigurationFromScene();
    }

    private void GetConfigurationFromScene(){
        ambientMode = RenderSettings.ambientMode;
        ambientColor = RenderSettings.ambientLight;
        skyboxMat = RenderSettings.skybox;
    }

    private void SetConfigurationFromAsset(){
        RenderSettings.ambientMode = data.mode;
        ambientMode = data.mode;

        if(data.mode == AmbientMode.Skybox){
            RenderSettings.skybox = data.skyboxMaterial;
            skyboxMat = data.skyboxMaterial;
        }

        else if(data.mode == AmbientMode.Flat){
            RenderSettings.ambientLight = data.ambientColor;
            ambientColor = data.ambientColor;
        }
    }

    private void CreateEnvironmentAsset(){
        EnvironmentSettingsAsset newAsset = ScriptableObject.CreateInstance<EnvironmentSettingsAsset>();

        newAsset.mode = ambientMode;
        newAsset.ambientColor = ambientColor;
        newAsset.skyboxMaterial = skyboxMat;

        AssetDatabase.CreateAsset(newAsset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newAsset;
    }

    public void OnGUI()
    {

        EditorGUILayout.LabelField("Ambient Mode");
        ambientMode = (AmbientMode)EditorGUILayout.EnumFlagsField(ambientMode);
        RenderSettings.ambientMode = ambientMode;

        data = (EnvironmentSettingsAsset)EditorGUILayout.ObjectField(data, typeof(EnvironmentSettingsAsset), true);
        if(data != null){
            if(table.ContainsKey(currentScene.name)){
                table[currentScene.name] = data;
            }
            else{
                table.Add(currentScene.name, data);
            }
        }

        if(ambientMode == AmbientMode.Skybox){
            skyboxMat = (Material)EditorGUILayout.ObjectField("Skybox Material", skyboxMat, typeof(Material), true);
            RenderSettings.skybox = skyboxMat;
        }

        else if(ambientMode == AmbientMode.Flat){
            EditorGUILayout.LabelField("Ambient Color");
            ambientColor = EditorGUILayout.ColorField(ambientColor);
            RenderSettings.ambientLight = ambientColor;
        }

        if(GUILayout.Button("Generate Evironment Settings Asset")){
            CreateEnvironmentAsset();
        }

        SaveChanges();
    } 

}
