using UnityEngine;
using UnityEngine.UI;
using Mirror;

public abstract class ConectorDeTransport: MonoBehaviour {
    public abstract void Setup();
    public abstract void Hostear(System.Action<bool> callback = null);
    public abstract void ConectarCliente(System.Action<bool> callback = null);
    public abstract void EncerrarHost();
    public abstract void EncerrarCliente();
}