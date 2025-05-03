using System.Collections;
using UnityEngine;

public class PonteLevadica : IResetavel
{
    public Quaternion rotDesejada = new Quaternion();
    private Quaternion rotInicial;
    [SerializeField] private float duracao;
    private Coroutine rotinaAnterior;


    private void Awake(){
        rotInicial = transform.rotation;
    }

    public override void OnReset(){
        StopAllCoroutines();
        transform.rotation = rotInicial;
    }

    public void AbaixarPonte(){
        if(rotinaAnterior != null){
            StopCoroutine(rotinaAnterior);
        }

        rotinaAnterior = StartCoroutine(InterpolarRotação(rotDesejada));
    }

    public void LevantarPonte(){
        if(rotinaAnterior != null){
            StopCoroutine(rotinaAnterior);
        }

        rotinaAnterior = StartCoroutine(InterpolarRotação(rotInicial));
    }

    IEnumerator InterpolarRotação( Quaternion fRot){

        float duracao = this.duracao;
        float tempo = 0f;
        Quaternion rotInicial = transform.rotation;

        while (tempo < duracao)
        {
            tempo += Time.deltaTime;
            float t = tempo / duracao;

            transform.rotation = Quaternion.Lerp(rotInicial, fRot, t);
            yield return new WaitForFixedUpdate();
        }

        transform.rotation = fRot;
    }
}
