using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Animations;

[RequireComponent(typeof(Rigidbody))]
public class Carregavel : MonoBehaviour, InteracaoCondicional {
    public UnityEvent onCarregado, onSolto;
    public System.Action<Carregador> OnCarregado, OnSolto; // Chamado quando o carregador carrega ou solta um objeto

    Rigidbody rb;
    bool _sendoCarregado = false;
    public bool sendoCarregado => _sendoCarregado;
    public Carregador carregador { get; private set; } // O carregador que está carregando o objeto, se houver
    [HideInInspector] public ParentConstraint parentConstraint {get; private set; }

    void Awake() {
        rb = GetComponent<Rigidbody>();

        parentConstraint = gameObject.GetComponent<ParentConstraint>();
        if (parentConstraint == null) parentConstraint = gameObject.AddComponent<ParentConstraint>();

        parentConstraint.rotationAxis = Axis.None; // Desabilita rotação ao ser pego
    }

    void OnDisable() {
        carregador?.Soltar();
    }

    /// <summary>
    /// Condições para que o jogador possa interagir com o objeto. 
    /// Se o jogador estiver carregando outro objeto, não poderá interagir com este.
    /// Se o objeto já estiver sendo carregado, não poderá interagir com ele.
    /// </summary>
    /// <param name="jogador">Jogador que interagiu</param>
    /// <returns>Positivo se pode ser interagido</returns>
    public bool PodeInteragir(Player jogador) {
        return PodeInteragir(jogador.carregador);
    }

    /// <summary>
    /// Condições para que o jogador possa interagir com o objeto. 
    /// Se o jogador estiver carregando outro objeto, não poderá interagir com este.
    /// Se o objeto já estiver sendo carregado, não poderá interagir com ele.
    /// </summary>
    /// <param name="carregador">Carregador que interagiu</param>
    /// <returns>Positivo se pode ser interagido</returns>
    public bool PodeInteragir(Carregador carregador) {
        return !carregador.estaCarregando && !_sendoCarregado;
    }

    /// <summary>
    /// Chamado quando algum jogador tenta interagir com o objeto.
    /// </summary>
    /// <param name="jogador">Jogador que interagiu</param>
    public void Interagir(Player jogador) {
        Carregar(jogador.carregador);
    }
    
    /// <summary>
    /// Carrega o objeto com o carregador passado como parâmetro. Se o objeto já estiver sendo carregado, não faz nada.
    /// </summary>
    /// <param name="carregador">Carregador que irá carregar o objeto</param>
    public void Carregar(Carregador carregador) {
        if (carregador.estaCarregando || _sendoCarregado) return;
        if (carregador.carregado != this && !carregador.Carregar(this)) return; // Se não conseguiu carregar, não faz nada

        this.carregador = carregador;

        onCarregado.Invoke();
        OnCarregado?.Invoke(carregador);
    }

    bool tinhaGravidade = false;

    /// <summary>
    /// Chamado automaticamente pelo Carregador quando o objeto é carregado
    /// </summary>
    public void HandleSendoCarregado() {
        _sendoCarregado = true;
        rb.isKinematic = true;
        tinhaGravidade = rb.useGravity;
        rb.useGravity = false; // Desabilita a gravidade enquanto o objeto estiver sendo carregado
    }

    /// <summary>
    /// Chamado automaticamente pelo Carregador quando o objeto é solto
    /// </summary>
    public void HandleSolto() {
        onSolto.Invoke();
        OnSolto?.Invoke(carregador);

        _sendoCarregado = false;
        rb.isKinematic = false;
        rb.useGravity = tinhaGravidade; // Restaura a gravidade
        this.carregador = null;
    }
}
