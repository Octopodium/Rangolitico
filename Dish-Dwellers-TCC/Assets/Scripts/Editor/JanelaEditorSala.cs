using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JanelaEditorSala : EditorWindow
{
    [Header("Environment Settings Asset")]
    [SerializeField] EnvironmentSettingsAsset data;
    private readonly string path = "Assets/EvironmentSettings/Data.asset";
    private string floorTag = "Floor";
    private string wallTag = "Wall";
    private string waterTag = "Water";


    private void OnEnable() {
        //SetDataByScene();
        EditorSceneManager.activeSceneChangedInEditMode += OnSceneChanged;
        
    }

    [MenuItem("Window/Editor de Sala")]
    public static void ShowWindow()
    {
        GetWindow(typeof(JanelaEditorSala));
    }


    private void OnSceneChanged(Scene previous, Scene next)
    {
        data = null;
    }

    private void SetConfigurationFromAsset()
    {
        Lightmapping.lightingSettings = data.lightingSettings;

        RenderSettings.ambientMode = data.mode;
        RenderSettings.ambientLight = data.ambientColor;
        RenderSettings.skybox = data.skyboxMaterial;

        RenderSettings.fog = data.fog;
        RenderSettings.fogColor = data.fogColor;
        RenderSettings.fogDensity = data.fogDensity;

        if (data.floorMaterial != null)
        {
            SetEnvironmentMaterial(data.floorMaterial, floorTag);
        }
        if (data.wallMaterial != null)
        {
            SetEnvironmentMaterial(data.wallMaterial, wallTag);
        }
        if (data.waterMaterial != null)
        {
            SetEnvironmentMaterial(data.waterMaterial, waterTag);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log("Settings applied to scene.");
    }

    private void SetEnvironmentMaterial(Material floorMat, string tag)
    {

        GameObject[] floor = GameObject.FindGameObjectsWithTag(tag);
        foreach (GameObject obj in floor)
        {
            Renderer render = obj.GetComponentInChildren<Renderer>();
            render.material = floorMat;
        }
    }

    private void CreateEnvironmentAsset()
    {
        EnvironmentSettingsAsset newAsset = ScriptableObject.CreateInstance<EnvironmentSettingsAsset>();

        newAsset.lightingSettings = Lightmapping.lightingSettings;

        newAsset.mode = RenderSettings.ambientMode;
        newAsset.ambientColor = RenderSettings.ambientLight;
        newAsset.skyboxMaterial = RenderSettings.skybox;

        newAsset.fog = RenderSettings.fog;
        newAsset.fogColor = RenderSettings.fogColor;
        newAsset.fogDensity = RenderSettings.fogDensity;

        AssetDatabase.CreateAsset(newAsset, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = newAsset;
    }

    private void SaveTags() {
        EditorPrefs.SetString("FloorTag", floorTag);
        EditorPrefs.SetString("WallTag", wallTag);
        EditorPrefs.SetString("WaterTag", waterTag);
    }

    private void LoadTags(){
        string floorVal = EditorPrefs.GetString("FloorTag");
        if (!string.IsNullOrEmpty(floorVal))
        {
            floorTag = EditorPrefs.GetString("FloorTag");
        }
        wallTag = EditorPrefs.GetString("WallTag");
        waterTag = EditorPrefs.GetString("WaterTag");
    }

    public void OnGUI() {
        EditorGUILayout.LabelField("Evironment Asset");

        EditorGUI.BeginChangeCheck();

        data = (EnvironmentSettingsAsset)EditorGUILayout.ObjectField(data, typeof(EnvironmentSettingsAsset), true);
        EditorGUILayout.Space();

        if (data != null) {
            floorTag = EditorGUILayout.TagField("Floor Tag", floorTag);
            wallTag = EditorGUILayout.TagField("Wall Tag", wallTag);
            waterTag = EditorGUILayout.TagField("Water Tag", waterTag);



            EditorGUILayout.Space();
        }

        if (EditorGUI.EndChangeCheck()) {
            SetConfigurationFromAsset();
            SaveTags();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Generate Evironment Settings Asset")) {
            CreateEnvironmentAsset();
        }

        SaveChanges();
    }

}
