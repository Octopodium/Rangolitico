using UnityEngine.Events;
using UnityEngine;

public class OnTriggerEvent : MonoBehaviour
{
    public UnityEvent OnColidir;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            OnColidir?.Invoke();
        }
    }
}
