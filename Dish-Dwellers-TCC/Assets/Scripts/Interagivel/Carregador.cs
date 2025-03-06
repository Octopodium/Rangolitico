using UnityEngine;

public class Carregador: MonoBehaviour {
    public Transform carregarTransform;
    public float forcaArremesso = 5f, alturaArremesso = 1.5f;
    [Range(0, 1)] public float influenciaDaInerciaNoArremesso = 0.33f;

    [HideInInspector] public Carregavel carregado;
    public bool estaCarregando => carregado != null;

    public void Carregar(Carregavel carregavel) {
        carregado = carregavel;
        carregavel.transform.SetParent(carregarTransform);
        carregavel.transform.localPosition = Vector3.zero;

        Rigidbody cargaRigidbody = carregavel.GetComponent<Rigidbody>();
        if (cargaRigidbody != null) {
            carregavel.HandleSendoCarregado();
        }
    }

    public void Soltar(Vector3 direcao, float velocidade, bool movendo = false) {
        carregado.transform.SetParent(null);

        Rigidbody cargaRigidbody = carregado.GetComponent<Rigidbody>();

        if (cargaRigidbody != null) {
            carregado.HandleSolto();

            if (movendo)
                cargaRigidbody.linearVelocity = influenciaDaInerciaNoArremesso * (direcao * velocidade);

            Vector3 arremeco = direcao;
            arremeco.y = alturaArremesso;
            cargaRigidbody.AddForce(arremeco * forcaArremesso, ForceMode.Impulse);
        }

        carregado = null;
    }

    public Vector3[] PreverArremesso(Rigidbody rigidbody, Vector3 direcao, float forca, Vector3 velocidadeInicial) {
        int quantidadeMaxPontos = 20;
        float tempo = 10 * Time.fixedDeltaTime; // Intervalos de tempo

        Vector3[] pontos = new Vector3[quantidadeMaxPontos];
        Vector3 posicao = rigidbody.position;
        Vector3 velocidade = direcao * forca + velocidadeInicial;
        Vector3 gravidade = 0.5f * Physics.gravity * tempo * tempo;

        for (int i = 0; i < quantidadeMaxPontos; i++) {
            posicao += velocidade * tempo + gravidade;
            velocidade += (Physics.gravity * tempo);
            pontos[i] = posicao;
        }

        return pontos;
    }
}
