using System.Collections;
using UnityEngine;
using UnityEngine.Events;


public interface IGanchavelAntesPuxar { IEnumerator GanchavelAntesPuxar(); }
public interface IGanchavelAntesGanchar { IEnumerator GanchavelAntesGanchar(); }
public interface IGanchavelAntesDesganchar { IEnumerator GanchavelAntesDesganchar(); }



public class Ganchavel : MonoBehaviour {
    public UnityEvent onGanchado, onDesganchado, onPuxado;
    Collider colisor;
    bool _ganchado = false;
    public bool ganchado { get { return _ganchado; } }

    public Vector3 meio { get { return colisor.bounds.center; } }

    void Awake() {
        colisor = GetComponent<Collider>();
    }

    public virtual bool PodeSerGanchado() {
        return true;
    }

    public void HandleGanchado() {
        _ganchado = true;
        onGanchado.Invoke();
    }

    public void HandleDesganchado() {
        _ganchado = false;
        onDesganchado.Invoke();
    }

    public void HandlePuxado() {
        _ganchado = false;
        onPuxado.Invoke();
    }
}
