using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Data", menuName = "Eviroment/EvironmentSettingsAsset")]
public class EnvironmentSettingsAsset : ScriptableObject
{

    [Header("Lighting")]
    public LightingSettings lightingSettings;

    [Header("Fog")]
    public bool fog;
    public Color fogColor;
    public float fogDensity;


    [Header("Evironment Lighting")]
    public AmbientMode mode;
    public Material skyboxMaterial;
    public Color ambientColor;

    [Header("Level Config")]
    public Material floorMaterial;
    public Material wallMaterial;
    public Material waterMaterial;
}
