using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
public class DropShadow : MonoBehaviour{

    [Space(10)][Header("<color=green>Configurações : </color>")]
    [Range(0.0f, 1.0f)] public float opacidade;
    public Color cor;
    public float tamanho = 1.0f;
    public bool compartilhado;
    
    private int idAlhpa = Shader.PropertyToID("_Alpha");
    private int idCor = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock mpb;


    private void OnValidate(){
        AtualizarTamanho();
        AtualizarCor();
    }

    private void AtualizarTamanho(){

        if(transform.localScale.x != tamanho){
            transform.localScale = Vector3.one * tamanho;
        }

    }

    private void AtualizarCor(){

        MeshRenderer renderer = GetComponent<MeshRenderer>();

        if(compartilhado){
            renderer.sharedMaterial.SetFloat(idAlhpa, opacidade);
            renderer.sharedMaterial.SetColor(idCor, cor);
        }
        else{
            mpb = new MaterialPropertyBlock();
            mpb.SetFloat(idAlhpa, opacidade);
            mpb.SetColor(idCor, cor);
            renderer.SetPropertyBlock(mpb);
        }

    }

}
