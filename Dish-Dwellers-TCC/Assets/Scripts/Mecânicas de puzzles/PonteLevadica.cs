using System.Collections;
using UnityEngine;

public class PonteLevadica : IResetavel
{
    [SerializeField] public Quaternion rotDesejada;
    private Quaternion rotInicial;
    [SerializeField] private float duracao;
    private float progresso = 0.0f;
    private bool rolando = false;


    private void Awake() {
        rotInicial = transform.rotation;
    }

    private void FixedUpdate() {

        if (rolando) {
            progresso += Time.fixedDeltaTime;
        }
        else if (rolando == false) {
            progresso -= Time.fixedDeltaTime;
        }
        progresso = Mathf.Clamp(progresso, 0, duracao);

        float t = progresso / duracao;
 
        transform.rotation = Quaternion.Lerp(rotInicial, rotDesejada, t);
    }

    public override void OnReset() {
        StopAllCoroutines();
        transform.rotation = rotInicial;
    }

    public void AbaixarPonte() {
        rolando = true;
    }

    public void LevantarPonte() {
        rolando = false;
    }
}
