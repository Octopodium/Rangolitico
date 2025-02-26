using UnityEngine;

public class Player : MonoBehaviour {
    [Header("Atributos do Player")]
    public float velocidade = 6f;

    [Header("Configuração de Interação")]
    public int maxInteragiveisEmRaio = 8;
    public float raioInteracao = 1f;
    public LayerMask layerInteragivel;
    Interagivel ultimoInteragivel;
    Collider[] collidersInteragiveis;

    CharacterController controller;

    void Awake() {
        controller = GetComponent<CharacterController>();
        collidersInteragiveis = new Collider[maxInteragiveisEmRaio];
    }

    void Start() {
        GameManager.instance.input.Player.Interact.performed += ctx => Interagir();
    }

    void FixedUpdate() {
        Movimentacao();
        CheckInteragiveis();
    }

    void Movimentacao() {
        Vector2 input = GameManager.instance.input.Player.Move.ReadValue<Vector2>();
        float x = input.x;
        float z = input.y;

        Vector3 movimentacao = transform.right * x + transform.forward * z;

        if (!controller.isGrounded) movimentacao.y = -9f;

        controller.Move(movimentacao * velocidade * Time.deltaTime);
    }
    
    void CheckInteragiveis() {
        // Checa por objetos interagíveis no raio de interação
        int quant = Physics.OverlapSphereNonAlloc(transform.position, raioInteracao, collidersInteragiveis, layerInteragivel);
        if (quant == 0) { // Na maioria das vezes, não haverá interagíveis
            if (ultimoInteragivel != null) ultimoInteragivel.MostarIndicador(false);
            ultimoInteragivel = null;
            return;
        }

        // Procura o interagível mais próximo (não podemos confiar na ordem padrão dos colliders)
        float menorDistancia = Mathf.Infinity;
        Collider interagivelMaisProximo = null;
        for (int i = 0; i < quant; i++) { // Na maioria das vezes, só haverá um interagível, e se houver mais, não será muitos (menos de 8)
            Collider collider = collidersInteragiveis[i];

            float distancia = Vector3.Distance(transform.position, collider.transform.position);
            if (distancia < menorDistancia) {
                menorDistancia = distancia;
                interagivelMaisProximo = collider;
            }
        }

        // Trata do ultimo interagivel
        if (ultimoInteragivel != null) {
            if (ultimoInteragivel.gameObject == interagivelMaisProximo.gameObject) return; // Se o interagível mais próximo for o mesmo que o último interagível, não faz nada
            ultimoInteragivel.MostarIndicador(false);
        }

        // Trata do novo interagivel mais proximo
        Interagivel interagivel = interagivelMaisProximo.GetComponent<Interagivel>();
        interagivel.MostarIndicador(true);
        ultimoInteragivel = interagivel;
    }

    void Interagir() {
        if (ultimoInteragivel != null) ultimoInteragivel.Interagir();
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, raioInteracao);
    }
}
