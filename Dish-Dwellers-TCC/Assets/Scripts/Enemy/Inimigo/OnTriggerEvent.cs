using UnityEngine.Events;
using UnityEngine;

public class OnTriggerEvent : MonoBehaviour
{
    public UnityEvent OnColidir;
    public bool eventoUnico = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            OnColidir?.Invoke();
            if(eventoUnico) gameObject.SetActive(false);
        }
    }
}
