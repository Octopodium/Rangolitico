using UnityEngine;
using UnityEngine.Events;

public class Ganchavel : MonoBehaviour {
    public UnityEvent onGanchado, onDesganchado, onPuxado;
    Collider colisor;

    public Vector3 meio { get { return colisor.bounds.center; } }

    void Awake() {
        colisor = GetComponent<Collider>();
    }

    public virtual bool PodeSerGanchado() {
        return true;
    }

    public void HandleGanchado() {
        onGanchado.Invoke();
    }

    public void HandleDesganchado() {
        onDesganchado.Invoke();
    }

    public void HandlePuxado() {
        onPuxado.Invoke();
    }
}
