using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "Data", menuName = "Eviroment/EvironmentSettingsAsset")]
public class EnvironmentSettingsAsset : ScriptableObject
{
    public AmbientMode mode;

    public Material skyboxMaterial;

    [Header("Evironment Lighting")]
    public Color ambientColor;
}
