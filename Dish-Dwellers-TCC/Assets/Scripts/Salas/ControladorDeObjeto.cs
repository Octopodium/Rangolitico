using UnityEngine;

public class ControladorDeObjeto : MonoBehaviour
{
    [Header("<color=green>Componentes : </color>")][Space(10)]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform respawnPos;

    [Space(15)][Header("Objeto controlado :")][Space(10)]
    [SerializeField] private GameObject objeto;


    private void Start(){
        sala salaAtual = GameManager.instance.salaAtual;

        if(!salaAtual.objetosSensiveis.Contains(this)){
            GameManager.instance.salaAtual.objetosSensiveis.Add(this);
        }
    }

    /// <summary>
    /// Caso não exista nenhum objeto atribuido ao campo do objeto controlado, instancia um novo objeto com base no prefab.
    /// </summary>
    public void Spawn(){
        if(objeto == null) objeto = Instantiate(prefab, respawnPos.position, transform.rotation);
    }


    /// <summary>
    /// Transporta o objeto controlado para o ponto de respawn atribuido no componente.
    /// </summary>
    public void Reposition(){
        objeto.transform.position = respawnPos.position;
    }
}
