using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Porta : IResetavel, Interacao {
    [Tooltip("Colisor que transporta o jogador quando destrancada")]
    [SerializeField] private GameObject portal;
    [SerializeField] private Animator animator;
    [SerializeField] private float delayParaAtivarOPortal = 1.0f;
    private bool destrancada;
    public bool trancada => !destrancada;
    public UnityEvent OnDestrancaPorta;



    private void Start() {
        portal.SetActive(false);
    }

    public override void OnReset() {
        Trancar();
        portal.GetComponent<Portal>().OnReset();
    }

    public void Interagir(Player jogador) {
        if (destrancada) {
            return;
        }

        if (jogador.carregando != null && jogador.carregando.CompareTag("Chave")) {
            Destrancar();
            // Retira a chave do jogador.

            jogador.carregando.gameObject.SetActive(false);
            jogador.carregador.carregado = null;
        }
    }

    public void Destrancar() {
        StartCoroutine(AbrirPorta());

        OnDestrancaPorta?.Invoke();

        // Previne que o jogador possa destrancar a porta duas vezes.
    }

    public void Trancar() {
        StopAllCoroutines();
        portal.SetActive(false);
        animator.SetBool("Aberta", false);
    }

    IEnumerator AbrirPorta() {
        animator.SetBool("Aberta", true);
        float timer = delayParaAtivarOPortal;

        while (timer > 0) {
            timer -= Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        portal.SetActive(true);
    }
}
