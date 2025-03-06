using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class Carregavel : MonoBehaviour, InteracaoCondicional {
    public UnityEvent onCarregado, onSolto;

    Rigidbody rb;
    bool _sendoCarregado = false;
    public bool sendoCarregado => _sendoCarregado;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    public bool PodeInteragir(Player jogador) {
        return !jogador.carregador.estaCarregando && !_sendoCarregado;
    }

    public void Interagir(Player jogador) {
        Carregar(jogador.carregador);
    }
    
    public void Carregar(Carregador carregador) {
        if (carregador.estaCarregando || _sendoCarregado) return;
        carregador.Carregar(this);
    }

    public void HandleSendoCarregado() {
        _sendoCarregado = true;
        rb.isKinematic = true;

        onCarregado.Invoke();
    }

    public void HandleSolto() {
        _sendoCarregado = false;
        rb.isKinematic = false;

        onSolto.Invoke();
    }
}
