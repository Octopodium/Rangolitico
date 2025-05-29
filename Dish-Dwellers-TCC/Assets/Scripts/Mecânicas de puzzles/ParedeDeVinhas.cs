using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ParedeDeVinhas : IResetavel {
    [SerializeField] private int integridade = 3;
    [SerializeField] private Color[] cores;
    private Renderer[] renderers;
    private Collider col;
    private MaterialPropertyBlock mpb;

    private void Start() {
        mpb = new MaterialPropertyBlock();
        renderers = GetComponentsInChildren<Renderer>();
        col = GetComponent<Collider>();
    }

    public override void OnReset() {
        AtivarVinhas(true);
    }

    private void AtivarVinhas(bool ativa) {
        foreach (Renderer render in renderers) {
            render.gameObject.SetActive(ativa);
        }

        col.enabled = ativa;

        integridade = 3;
        SetarCor(integridade);
    }

    public void ReduzirIntegridade() {
        if (--integridade <= 0) {
            AtivarVinhas(false);
            return;
        }
        SetarCor(integridade);
    }

    private void SetarCor(int integridade) {
        mpb.SetColor("_BaseColor", cores[integridade - 1]);

        foreach (Renderer render in renderers) {
            render.SetPropertyBlock(mpb);
        }
    }

}
