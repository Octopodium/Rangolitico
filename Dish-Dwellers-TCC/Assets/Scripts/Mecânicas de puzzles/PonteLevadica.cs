using System.Collections;
using UnityEngine;

public class PonteLevadica : IResetavel
{
    [SerializeField] public Quaternion rotDesejada;
    private Quaternion rotInicial;
    [SerializeField] private float duracao;
    private Coroutine rotinaAnterior;
    private float progresso = 0.0f;
    private int ativacoes = 0;
    
    Quaternion step = Quaternion.Euler(new Vector3(0, 0, 18.0f));


    private void Awake() {
        rotInicial = transform.rotation;
    }

    public override void OnReset(){
        StopAllCoroutines();
        transform.rotation = rotInicial;
    }

    public void AbaixarPonte(){
        ativacoes++;
        if (rotinaAnterior != null) {
            StopCoroutine(rotinaAnterior);
        }

        rotinaAnterior = StartCoroutine(InterpolarRotação(true));
    }

    public void LevantarPonte(){
        --ativacoes;
        if (ativacoes > 0) {
            return;    
        }

        if (rotinaAnterior != null) {
            StopCoroutine(rotinaAnterior);
        }

        rotinaAnterior = StartCoroutine(InterpolarRotação(false));
    }

    IEnumerator InterpolarRotação(bool desce) {
        //Quaternion rotInicial = transform.rotation;
        float multiplicador = desce ? 1 : -1;

        while (progresso >= 0.0f && progresso <= duracao) {

            progresso += Time.fixedDeltaTime * multiplicador;
            float t = progresso / duracao;

            transform.rotation = Quaternion.Lerp(rotInicial, rotDesejada, t);
            yield return new WaitForFixedUpdate();
        }

        if (desce) {
            transform.rotation = rotDesejada;
            progresso = duracao;
        } else {
            transform.rotation = rotInicial;
            progresso = 0.0f;
        }
    }
}
