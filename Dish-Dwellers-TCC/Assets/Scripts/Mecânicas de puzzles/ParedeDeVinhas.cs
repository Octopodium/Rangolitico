using UnityEngine;

public class ParedeDeVinhas : MonoBehaviour
{
    [SerializeField] private int integridade = 3;
    [SerializeField] private Color[] cores;
    private MaterialPropertyBlock mpb;

    public void ReduzirIntegridade(){
        if(--integridade <= 0 ){
            Destroy(gameObject);
        }
    }

    private void SetarCor(int integridade){
        mpb = new MaterialPropertyBlock();
        Renderer renderer = GetComponent<Renderer>();

        mpb.SetColor("_Color",cores[integridade - 1]);
        renderer.SetPropertyBlock(mpb);
    }

}
