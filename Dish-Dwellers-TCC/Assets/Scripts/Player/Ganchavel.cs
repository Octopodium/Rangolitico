using UnityEngine;
using UnityEngine.Events;

public class Ganchavel : MonoBehaviour {
    public UnityEvent onGanchado, onDesganchado, onPuxado;

    public virtual bool PodeSerGanchado() {
        return true;
    }

    public virtual void HandleGanchado() {
        onGanchado.Invoke();
    }
}
