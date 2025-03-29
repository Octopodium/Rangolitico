using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
public class DropShadow : MonoBehaviour{

    [Space(10)][Header("<color=green>Configurações : </color>")]
    [Range(0.0f, 1.0f)] public float opacidade;
    public Color corJogador1, corJogador2;
    public float tamanho = 1.0f;
    public bool compartilhado;
    
    private int idAlhpa = Shader.PropertyToID("_Alpha");
    private int idCor = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock mpb;


    private void OnValidate(){
        AtualizarTamanho();

        int jogador = GetQualJogador();
        if(jogador == 1){
            AtualizarCor(corJogador1);
        }
        else if(jogador == 2){
            AtualizarCor(corJogador2);
        }
    }

    private void AtualizarTamanho(){

        if(transform.localScale.x != tamanho){
            transform.localScale = Vector3.one * tamanho;
        }

    }

    private int GetQualJogador(){
        Player player;
        if(player = GetComponentInParent<Player>()){
            if(player.qualPlayer == QualPlayer.Player1) return 1;
            else return 2;
        }
        else return 0;
    }

    private void AtualizarCor(Color cor){

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
