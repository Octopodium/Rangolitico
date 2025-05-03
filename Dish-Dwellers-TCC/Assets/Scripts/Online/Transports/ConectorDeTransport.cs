using UnityEngine;
using UnityEngine.UI;
using Mirror;

public abstract class ConectorDeTransport: MonoBehaviour {
    public abstract void Setup();
    public abstract void LogarUsuario(System.Action<bool> callback = null);
    public abstract void Hostear();
    public abstract void ConectarCliente();
    public abstract void EncerrarHost();
    public abstract void EncerrarCliente();
}