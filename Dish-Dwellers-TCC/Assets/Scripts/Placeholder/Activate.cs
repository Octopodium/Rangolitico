using UnityEngine;

public class ActivateObject : MonoBehaviour
{
    public void SetActiveState(bool state)
    {
        gameObject.SetActive(state);
    }

    public void ToggleActiveState()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
