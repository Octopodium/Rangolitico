using UnityEngine.Events;
using UnityEngine;

public class OnTriggerEvent : MonoBehaviour
{
    public UnityEvent OnColidir;
    public bool eventoUnico = false;


    private void Start()
    {
        sala sala = GameObject.FindWithTag("Sala").GetComponent<sala>();
        if(!sala.triggers.Contains(this)){
            sala.triggers.Add(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            OnColidir?.Invoke();
            if(eventoUnico) gameObject.SetActive(false);
        }
    }
}
