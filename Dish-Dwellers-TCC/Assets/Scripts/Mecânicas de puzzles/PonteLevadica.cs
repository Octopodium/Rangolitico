using UnityEngine;

public class PonteLevadica : IResetavel {
    [SerializeField] public Quaternion rotDesejada;
    [SerializeField] private float duracao;
    [SerializeField] private Renderer[] decals;
    private MaterialPropertyBlock mpb;
    private Quaternion rotInicial;
    private float progresso = 0.0f;
    private int ativacoes = 0;
    [SerializeField][ColorUsage(true, true)] private Color corAtivado, corDesativado;


    private void Awake() {
        rotInicial = transform.rotation;
        mpb = new MaterialPropertyBlock();
    }

    private void FixedUpdate() {

        if (ativacoes > 0) {
            progresso += Time.fixedDeltaTime;
        }
        else if (ativacoes <= 0) {
            progresso -= Time.fixedDeltaTime;
        }
        progresso = Mathf.Clamp(progresso, 0, duracao);

        float t = progresso / duracao;

        transform.rotation = Quaternion.Lerp(rotInicial, rotDesejada, t);
    }

    public override void OnReset() {
        transform.rotation = rotInicial;
    }

    private void TrocarCorDoDecalque(Color col) {
        mpb.SetColor("_EmissionColor", col);

        if (decals != null) {
            foreach (Renderer render in decals) {
                render.SetPropertyBlock(mpb);
            }
        }
    }

    public void AbaixarPonte() {
        ativacoes++;
        ChecarAtivacao();
    }

    public void LevantarPonte() {
        ativacoes--;
        ChecarAtivacao();
    }
    
    private void ChecarAtivacao() {
        if (ativacoes > 0) {
            TrocarCorDoDecalque(corAtivado);
        }
        else {
            TrocarCorDoDecalque(corDesativado);
        }
    }
}
