using UnityEngine.Events;
using UnityEngine;

public class OnTriggerEvent : MonoBehaviour
{
    public UnityEvent OnColidir;
    public bool eventoUnico = false;


    private void Start()
    {
        sala sala = GameManager.instance.PegarSalaDaCena(gameObject.scene);

        if(!sala.triggers.Contains(this)){
            sala.triggers.Add(this);
        }

        if (gameObject.layer == 0)
            gameObject.layer = LayerMask.NameToLayer("Trigger");
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
