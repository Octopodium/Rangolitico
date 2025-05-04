using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
public class DropShadow : MonoBehaviour{

    [Space(10)][Header("<color=green>Configurações : </color>")]
    [Range(0.0f, 1.0f)] public float opacidade;
    public Color corJogador1, corJogador2;
    public float tamanho = 1.0f;
    public bool compartilhado;
    [SerializeField] private LayerMask layerDoChao;
    [SerializeField] private Transform playerTransform;
    
    const float offset = 0.025f;
    private int idAlhpa = Shader.PropertyToID("_Alpha");
    private int idCor = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock mpb;

    private void Start(){
        Setup();
    }

    private void OnValidate(){
        Setup();
    }

    private void LateUpdate(){
        GrudarNoChao();
    }

    private void GrudarNoChao(){
        if(Physics.Raycast(playerTransform.position + Vector3.up, Vector3.down, out RaycastHit hitInfo, 10f, layerDoChao)){
            Vector3 novaPos = playerTransform.position;
            novaPos.y = hitInfo.point.y + offset;
            transform.position = novaPos;
            transform.up = hitInfo.normal;
        }
    }

    private void Setup(){
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
