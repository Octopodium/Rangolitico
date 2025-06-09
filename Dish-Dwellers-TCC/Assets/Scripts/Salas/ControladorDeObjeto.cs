using System.Collections;
using Mirror;
using UnityEngine;

public class ControladorDeObjeto : IResetavel, SincronizaMetodo {
    [Header("<color=green>Componentes : </color>")]
    [Space(10)]
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject prefabOnline;
    [SerializeField] private Transform respawnPos;

    [Space(15)]
    [Header("Objeto controlado :")]
    [Space(10)]
    private GameObject objeto;

    [Space(15)]
    [Header("Configurações")]
    [Space(10)]
    [SerializeField] private bool spawnNoInicio = false;


    private void Start() {

        if (spawnNoInicio)
            Spawn();
    }

    void OnDestroy() {
        if (objeto != null) {
            Destrutivel destrutivel = objeto.GetComponent<Destrutivel>();
            if (destrutivel != null) {
                destrutivel.OnDestruido.RemoveListener(Respawn);
            }

            Destroy(objeto);
            objeto = null;
        }
    }

    public override void OnReset() {
        Reiniciar();
    }

    /// <summary>
    /// Caso não exista nenhum objeto atribuido ao campo do objeto controlado, instancia um novo objeto com base no prefab.
    /// </summary>
    [Sincronizar]
    public void Spawn() {
        if (objeto != null) return;

        gameObject.Sincronizar();
        
        GameObject prefabToUse = GameManager.instance.isOnline ? prefabOnline : prefab;
        if (!GameManager.instance.isOnline) objeto = Instantiate(prefabToUse, respawnPos.position, transform.rotation);
        else {
            Sincronizador.instance.InstanciarNetworkObject(AposSpawn, prefabToUse, respawnPos.position, transform.rotation, null, true);
            return;
        }

        AposSpawn(objeto);
    }

    void AposSpawn(GameObject objeto) {
        if (objeto != null) {
            Destrutivel destrutivel = objeto.GetComponent<Destrutivel>();
            destrutivel.OnDestruido.AddListener(Respawn);
            this.objeto = objeto;
        }
    }


    /// <summary>
    /// Transporta o objeto controlado para o ponto de respawn atribuido no componente e ativa ele.
    /// </summary>
    [Sincronizar]
    public void Respawn(){
        gameObject.Sincronizar();
        objeto.transform.position = respawnPos.position;

        if(!objeto.activeInHierarchy)
            objeto.SetActive(true);
    }

    /// <summary>
    /// Destroi o objeto controlado e reinicia o sistema
    /// </summary>
    [Sincronizar]
    public void Reiniciar() {
        gameObject.Sincronizar();

        if (objeto != null) {
            // Essa parte é exclusiva pra esse código, instanciar e desinstanciar 
            Sincronizavel sincronizavel = objeto.GetComponent<Sincronizavel>();
            if (sincronizavel != null) {
                sincronizavel.PreDestroy();
            }

            Destroy(objeto);
            objeto = null;
        }
        

        if (spawnNoInicio)
            Spawn();
    }

}
