using UnityEngine;

public class ActivateObject : MonoBehaviour
{
    // Assign this in the inspector
    public GameObject targetObject;

    // Set to true to activate, false to deactivate
    public bool activate = true;

    void Start()
    {
        if (targetObject != null)
        {
            targetObject.SetActive(activate);
        }
        else
        {
            Debug.LogWarning("Target Object is not assigned!");
        }
    }
}