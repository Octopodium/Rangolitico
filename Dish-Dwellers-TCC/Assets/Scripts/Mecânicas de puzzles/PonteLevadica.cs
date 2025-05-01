using System.Collections;
using UnityEngine;

public class PonteLevadica : MonoBehaviour
{
    public Quaternion rotDesejada;
    private Quaternion rotInicial;
    [SerializeField] private float duracao = 3.0f;


    private void Awake(){
        rotInicial = transform.rotation;
    }

    private void Start()
    {
        AbaixarPonte();   
    }

    public void AbaixarPonte(){
        StartCoroutine(InterpolarRotação(rotInicial, rotDesejada));
    }

    public void LevantarPonte(){
        StartCoroutine(InterpolarRotação(rotDesejada, rotInicial));
    }

    IEnumerator InterpolarRotação(Quaternion oRot, Quaternion fRot){
        float t = 0.0f;
        while(t < duracao){
            t += Time.deltaTime;
            transform.rotation = Quaternion.Lerp(oRot, fRot, t/duracao);
            yield return null;
        }
        transform.rotation = rotDesejada;
    }
}
