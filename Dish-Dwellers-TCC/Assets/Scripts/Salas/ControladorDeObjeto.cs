using System.Collections;
using UnityEngine;

public interface IRecebeTemplate {
    void RecebeTemplate(GameObject template);
}


[RequireComponent(typeof(Sincronizavel))]
public class ControladorDeObjeto : IResetavel, SincronizaMetodo {
    [Header("</color=green>Componentes : </color>")]
    [Space(10)]
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject prefabOnline;
    public Vector3 respawnPos = new Vector3(0, 6, 0);
    [SerializeField] private GameObject triggerSpawnChave;

    [Space(15)]
    [Header("Objeto controlado :")]
    [Space(10)]
    public GameObject objeto;

    [Space(15)]
    [Header("Configurações")]
    [Space(10)]
    [SerializeField] private float dellay = 0.0f;
    [SerializeField] private bool habilitado = true;
    [SerializeField] private bool spawnNoInicio = false;
    [Tooltip("Caso o objeto prefab seja 'IRecebeTemplate', ele recebera o objeto 'template' como parâmetro de 'RecebeTemplate'. Util para replicar valores base nos objetos recém instanciados.")]
    public GameObject template;

    Sincronizavel sinc;

    bool spawnando = false;


    private void Start() {
        if (template != null) template.SetActive(false);

        sinc = GetComponent<Sincronizavel>();
        SetupSpawner();

        if (spawnNoInicio)
            sinc.ComSincronizador(Spawn);
    }

    bool spawnerSetted = false;
    void SetupSpawner() {
        if (!GameManager.instance.isOnline) return;
        if (spawnerSetted) return;
        spawnerSetted = true;

        // Debug.Log("Setupado o " + gameObject.name + " de " + prefab.name);

        GameObject prefabToUse = prefabOnline != null ? prefabOnline : prefab;
        if (sinc == null) sinc = GetComponent<Sincronizavel>();
        sinc.onObjetoSpawnado += AposSpawn;
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
        if (GameManager.instance.isOnline) {
            if (sinc == null) sinc = GetComponent<Sincronizavel>();
            sinc.ComSincronizador(()=> gameObject.NaoSincronizar(Reiniciar));
        } else Reiniciar();
    }

    /// <summary>
    /// Caso não exista nenhum objeto atribuido ao campo do objeto controlado, instancia um novo objeto com base no prefab.
    /// </summary>
    [Sincronizar]
    public void Spawn() {
        // if(!habilitado) return;
        // if (objeto != null) return;
        // if (spawnando) return;

        // gameObject.Sincronizar();
        
        // if (!GameManager.instance.isOnline) {
        //     spawnando = true;
        //     AposSpawn(Instantiate(prefab, transform.TransformPoint(respawnPos), transform.rotation));
        // }
        // else {
        //     SetupSpawner();
        //     GameObject prefabToUse = prefabOnline != null ? prefabOnline : prefab;
        //     if (Sincronizador.instance.InstanciarNetworkObject(prefabToUse, sinc))
        //         spawnando = true;
        // }
        
        if(!habilitado) return;
        if (objeto != null) return;
        if (spawnando) return;
        gameObject.Sincronizar();
        
        StartCoroutine(SpawnCoroutine());
    }

    IEnumerator SpawnCoroutine(){
        yield return new WaitForSecondsRealtime(dellay);
        if(!habilitado) yield break;
        if (objeto != null) yield break;
        if (spawnando) yield break;

        // Debug.Log("Spawnando " + prefab.name);
        
        if (!GameManager.instance.isOnline) {
            spawnando = true;
            AposSpawn(Instantiate(prefab, transform.TransformPoint(respawnPos), transform.rotation));
        }
        else {
            SetupSpawner();
            GameObject prefabToUse = prefabOnline != null ? prefabOnline : prefab;
            if (Sincronizador.instance.InstanciarNetworkObject(prefabToUse, sinc, transform.TransformPoint(respawnPos), transform.rotation))
                spawnando = true;
        }
    }

    void AposSpawn(GameObject objeto) {
        // Debug.Log("Spawnou " + objeto, objeto);
        if (objeto != null)
            spawnando = false;

        if (objeto != null && objeto != this.objeto) {
            if (this.objeto != null) {
                Destroy(this.objeto);
            }


            Destrutivel destrutivel = objeto.GetComponent<Destrutivel>();
            destrutivel?.OnDestruido.AddListener(Respawn);
            this.objeto = objeto;

            if (template != null) {
                IRecebeTemplate recebeTemplate = objeto.GetComponent<IRecebeTemplate>();
                if (recebeTemplate != null) {
                    recebeTemplate.RecebeTemplate(template);
                }
            }
        }
    }


    /// <summary>
    /// Transporta o objeto controlado para o ponto de respawn atribuido no componente e ativa ele.
    /// </summary>
    [Sincronizar]
    public void Respawn(){
        // gameObject.Sincronizar();
        
        // if (objeto != null) {
        //     objeto.transform.position = transform.TransformPoint(respawnPos);

        //     // Perseguidor perseguidor = objeto.GetComponent<Perseguidor>();
        //     // if (perseguidor != null) {
        //     //     perseguidor.ResetarParaEstadoInicial();
        //     // }

        //     if(!objeto.activeInHierarchy)
        //         objeto.SetActive(true);
        // } else {
        //     Spawn();
        // }

        gameObject.Sincronizar();
        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine() {
        // Debug.Log("Calling respawn of " + prefab.name);
        yield return new WaitForSecondsRealtime(dellay);
        
        if (objeto != null) {
            // Debug.Log("Respawn of " + prefab.name + " objeto ja existia!");
            objeto.transform.position = transform.TransformPoint(respawnPos);

            // Perseguidor perseguidor = objeto.GetComponent<Perseguidor>();
            // if (perseguidor != null) {
            //     perseguidor.ResetarParaEstadoInicial();
            // }

            if(!objeto.activeInHierarchy)
                objeto.SetActive(true);
        } else {
            // Debug.Log("Respawn of " + prefab.name + " objeto não existia!");
            Spawn();
        }
        
    }

    /// <summary>
    /// Destroi o objeto controlado e reinicia o sistema
    /// </summary>
    [Sincronizar]
    public void Reiniciar() {
        if (spawnando) return;
    
        gameObject.Sincronizar();

        // Debug.Log("Reiniciar of " + prefab.name);
        if (objeto != null) {
            // Debug.Log("Reiniciar of " + prefab.name + " objeto ja existia!");
            // Essa parte é exclusiva pra esse código, instanciar e desinstanciar 
            objeto.SetActive(false);

            Sincronizavel sincronizavel = objeto.GetComponent<Sincronizavel>();
            if (sincronizavel != null) {
                sincronizavel.PreDestroy();
            }

            Destroy(objeto);
            objeto = null;
        }

        if (spawnNoInicio) {
            // Debug.Log("Reiniciar on inicio of " + prefab.name);
            if (sinc == null) sinc = GetComponent<Sincronizavel>();
            sinc.ComSincronizador(Spawn);
        }
    }

}
