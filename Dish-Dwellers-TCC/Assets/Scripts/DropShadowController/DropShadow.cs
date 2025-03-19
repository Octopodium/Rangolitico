using UnityEngine;
[RequireComponent(typeof(MeshRenderer))]
public class DropShadow : MonoBehaviour
{
    [Space(10)][Header("<color=green>Configurações da sombra : ")]
    [SerializeField] private Color cor;
    [SerializeField] private float tamanho = 1.0f;
    [Range(0.0f, 1.0f)][SerializeField] private float transparencia;
    private int idAlhpa = Shader.PropertyToID("_Alpha");
    private int idCor = Shader.PropertyToID("_Color");

    private void OnValidate(){
        if(transform.localScale.x != tamanho){
            transform.localScale = Vector3.one * tamanho;
        }
        
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.sharedMaterial.SetFloat(idAlhpa, transparencia);
        renderer.sharedMaterial.SetColor(idCor, cor);
    }
}
