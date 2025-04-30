using UnityEngine;

public class ControladorDeObjeto : MonoBehaviour
{
    [Header("<color=green>Componentes : </color>")][Space(10)]
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform respawnPos;

    [Space(15)][Header("Objeto controlado :")][Space(10)]
    private GameObject objeto;

    [Space(15)][Header("Configurações")][Space(10)]
    [SerializeField] private bool spawnNoInicio = false;


    private void Start(){
        sala salaAtual = GameObject.FindWithTag("Sala").GetComponent<sala>();

        if(!salaAtual.objetosSensiveis.Contains(this)){
            GameManager.instance.salaAtual.objetosSensiveis.Add(this);
        }

        if(spawnNoInicio) 
            Spawn();
    }

    /// <summary>
    /// Caso não exista nenhum objeto atribuido ao campo do objeto controlado, instancia um novo objeto com base no prefab.
    /// </summary>
    public void Spawn(){
        if(objeto == null) objeto = Instantiate(prefab, respawnPos.position, transform.rotation);

        Destrutivel destrutivel = objeto.GetComponent<Destrutivel>();
        destrutivel.OnDestruido.AddListener(Respawn);
    }


    /// <summary>
    /// Transporta o objeto controlado para o ponto de respawn atribuido no componente e ativa ele.
    /// </summary>
    public void Respawn(){
        objeto.transform.position = respawnPos.position;

        if(!objeto.activeInHierarchy)
            objeto.SetActive(true);
    }

    /// <summary>
    /// Destroi o objeto controlado e reinicia o sistema
    /// </summary>
    public void Reiniciar(){
        Destroy(objeto);

        if(spawnNoInicio) 
            Spawn();
    }

}
