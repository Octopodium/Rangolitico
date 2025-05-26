using UnityEngine;

public class ParedeDeVinhas : MonoBehaviour
{
    [SerializeField] private int integridade = 3;
    [SerializeField] private Color[] cores;
    [SerializeField] private SkinnedMeshRenderer[] renderers;
    private MaterialPropertyBlock mpb;

    private void Start() {
        mpb = new MaterialPropertyBlock();
    }

    public void ReduzirIntegridade() {
        if (--integridade <= 0) {
            Destroy(gameObject);
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
