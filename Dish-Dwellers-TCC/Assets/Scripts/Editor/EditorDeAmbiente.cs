using UnityEngine;

public class EditorDeAmbiente : MonoBehaviour
{

    private void OnEnable()
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.red;
    }
}
