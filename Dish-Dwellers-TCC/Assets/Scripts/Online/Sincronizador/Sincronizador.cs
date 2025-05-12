using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;

/*
    EI, VOCÊ AÍ!
    Se você estiver utilizando o atributo [Sincronizar], é melhor você olhar o arquivo Sincronizavel.cs
    Este código é a versão no pelo, só pra quem sabe o que está fazendo.

    "Ai mas eu não sei nenhum dos dois", ok, então vai pro outro!
    "Mas eu não quero, eu quero fazer no pelo!", vai por sua conta e risco então camarada.
*/
/*
public interface SincronizadorDeTipo<T> {
    void SetTrigger(string triggerName, T obj);
    System.Action<object> WrapActionObject(System.Action<T> action);
}
*/

public class Sincronizador : NetworkBehaviour {
    public static Sincronizador instance { get; private set; }
    public static System.Action onInstanciaCriada;

    public static System.Type[] tiposSuportados = new System.Type[] { typeof(int), typeof(GameObject) };


    protected Dictionary<string, Sincronizavel> sincronizaveis = new Dictionary<string, Sincronizavel>();
    protected Dictionary<string, List<System.Action>> triggers = new Dictionary<string, List<System.Action>>();
    protected Dictionary<string, List<System.Action<object>>> triggersComParametro = new Dictionary<string, List<System.Action<object>>>();

    bool isOnCallback = false;

    public struct SincronizarTriggerMessage : NetworkMessage {
        public string trigger;

        public SincronizarTriggerMessage(string trigger) {
            this.trigger = trigger;
        }
    }

    public struct SincronizarIntTriggerMessage : NetworkMessage {
        public string trigger;
        public int valor;

        public SincronizarIntTriggerMessage(string trigger, int valor) {
            this.trigger = trigger;
            this.valor = valor;
        }
    }

    public struct SincronizarStringTriggerMessage : NetworkMessage {
        public string trigger;
        public string valor;

        public SincronizarStringTriggerMessage(string trigger, string valor) {
            this.trigger = trigger;
            this.valor = valor;
        }
    }


    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }


        NetworkServer.RegisterHandler<SincronizarTriggerMessage>(ServerOnSetTrigger);
        NetworkServer.RegisterHandler<SincronizarIntTriggerMessage>(ServerOnSetTrigger);
        NetworkServer.RegisterHandler<SincronizarStringTriggerMessage>(ServerOnSetTrigger);
  
        onInstanciaCriada?.Invoke();
        onInstanciaCriada = null;
    }

    public void ForeachConnection(System.Action<NetworkConnectionToClient> action, NetworkConnectionToClient except = null) {
        foreach (var conn in NetworkServer.connections) {
            NetworkConnectionToClient conexao = conn.Value;
            if (conexao == null || conexao == except) continue;
            action.Invoke(conexao);
        }
    }

    public void ForeachTriggerComParametro(string triggerName, System.Action<System.Action<object>> action) {
        isOnCallback = true;

        if (triggersComParametro.ContainsKey(triggerName)) {
            foreach (var acao in triggersComParametro[triggerName]) {
                action.Invoke(acao);
            }
        }

        isOnCallback = false;
    }


    /// <summary>
    /// Chame essa função para ter certeza que o ID do objeto sincronizável é único.
    /// Caso o ID já exista, ele irá adicionar um sufixo "_1", "_2", etc. até encontrar um ID único.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public string CertificarIDSincronizavel(string id, string prefixo = "", string sufixo = "") {
        string idValidado = id;

        int i = 1;
        while (sincronizaveis.ContainsKey(prefixo+idValidado+sufixo)) {
            idValidado = id + "_" + i;
            i++;
        }

        return idValidado;
    }

    public bool CadastrarSincronizavel(Sincronizavel obj) {
        string idOriginal = obj.GetID();
        string id = idOriginal;

        if (sincronizaveis.ContainsKey(id)) {
            Debug.LogWarning("Sincronizavel de ID ["+id+"] não foi cadastrado pois já havia um com o mesmo ID!");
            return false;
        }

        sincronizaveis[id] = obj;
        return true;
    }

    public void DescadastrarSincronizavel(Sincronizavel obj) {
        string id = obj.GetID();

        if (sincronizaveis.ContainsKey(id)) 
            sincronizaveis.Remove(id);
    }

    public Sincronizavel GetSincronizavel(string id) {
        if (sincronizaveis.ContainsKey(id)) {
            return sincronizaveis[id];
        } else {
            return null;
        }
    }

    #region Set e Unset de Trigger

    public void OnTrigger(string triggerName, System.Action action) {
        if (!CanSetOnTrigger(triggerName)) return;

        if (!triggers.ContainsKey(triggerName)) {
            triggers[triggerName] = new List<System.Action>();
        }

        triggers[triggerName].Add(action);
    }

    public void OnTrigger(string triggerName, System.Action<object> action) {
        if (!CanSetOnTrigger(triggerName)) return;

        if (!triggersComParametro.ContainsKey(triggerName)) {
            triggersComParametro[triggerName] = new List<System.Action<object>>();
        }

        triggersComParametro[triggerName].Add(action);
    }

    public void OffTrigger(string triggerName, System.Action action) {
        if (!CanSetOnTrigger(triggerName)) return;

        if (triggers.ContainsKey(triggerName)) {
            triggers[triggerName].Remove(action);
        }
    }

    public void OffTrigger(string triggerName, System.Action<object> action) {
        if (!CanSetOnTrigger(triggerName)) return;

        if (triggersComParametro.ContainsKey(triggerName)) {
            triggersComParametro[triggerName].Remove(action);
        }
    }

    public bool CanSetTrigger(string triggerName) {
        return !isOnCallback && GameManager.instance != null && GameManager.instance.isOnline && triggerName != null;
    }

    public bool CanSetOnTrigger(string triggerName) {
        return GameManager.instance != null && GameManager.instance.isOnline && triggerName != null;
    }

    #endregion


    // Sincronização de triggers

    #region Sincronização sem parametros

    public void SetTrigger(string triggerName) {
        if (!CanSetTrigger(triggerName)) return;

        NetworkClient.Send(new SincronizarTriggerMessage(triggerName));
    }

    [Server]
    public void ServerOnSetTrigger(NetworkConnectionToClient quemChamou, SincronizarTriggerMessage triggerMessage) {
        string triggerName = triggerMessage.trigger;
        ForeachConnection((conexao) => {
            TargetSetTrigger(conexao, triggerName);
        }, quemChamou);
    }

    [TargetRpc]
    public void TargetSetTrigger(NetworkConnectionToClient target, string triggerName) {
        isOnCallback = true;

        if (triggers.ContainsKey(triggerName)) {
            foreach (var action in triggers[triggerName]) {
                action.Invoke();
            }
        }

        isOnCallback = false;
    }

    #endregion
    
    #region Sincronização com parametro <INT>

    public void SetTrigger(string triggerName, int valor) {
        if (!CanSetTrigger(triggerName)) return;

        NetworkClient.Send(new SincronizarIntTriggerMessage(triggerName, valor));
    }

    [Server]
    private void ServerOnSetTrigger(NetworkConnectionToClient quemChamou, SincronizarIntTriggerMessage triggerMessage) {
        string triggerName = triggerMessage.trigger;

        ForeachConnection((conexao) => {
            TargetSetTrigger(conexao, triggerName, triggerMessage.valor);
        }, quemChamou);
    }

    [TargetRpc]
    private void TargetSetTrigger(NetworkConnectionToClient target, string triggerName, int valor) {
        ForeachTriggerComParametro(triggerName, (action) => {
            action.Invoke(valor);
        });
    }

    #endregion

    #region Sincronização com parametro <GAMEOBJECT> (nota: Deve possuir o componente Sincronizavel)
    
    public void SetTrigger(string triggerName, GameObject obj) {
        if (!CanSetTrigger(triggerName)) return;

        Sincronizavel sincronizavel = obj.GetComponent<Sincronizavel>();
        if (sincronizavel == null && sincronizavel.GetID().Trim() == "") {
            Debug.LogError("Para sincronizar um parâmetro <GameObject>, é necessário que este possua o componente <Sincronizavel> com um id único.");
            return;
        }

        string id = sincronizavel.GetID();
        NetworkClient.Send(new SincronizarStringTriggerMessage(triggerName, id));
    }

    [Server]
    private void ServerOnSetTrigger(NetworkConnectionToClient quemChamou, SincronizarStringTriggerMessage triggerMessage) {
        string triggerName = triggerMessage.trigger;

        ForeachConnection((conexao) => {
            TargetSetTrigger(conexao, triggerName, triggerMessage.valor);
        }, quemChamou);
    }

    [TargetRpc]
    private void TargetSetTrigger(NetworkConnectionToClient target, string triggerName, string valor) {

        if (triggersComParametro.ContainsKey(triggerName)) {
            Sincronizavel sincronizavel = sincronizaveis.ContainsKey(valor) ? sincronizaveis[valor] : null;
            if (sincronizavel != null) {
                GameObject obj = sincronizavel.gameObject;
                ForeachTriggerComParametro(triggerName, (action) => {
                    action.Invoke(obj);
                });
            }
        }
    }

    #endregion

    

    #region Conversores

    public System.Action WrapActionObjectSemParametro(System.Action action, bool debugLog = false) {
        if (action == null) return null;
        return () => {
            if (debugLog) Debug.Log("[Sync - Inicio]");

            action.Invoke();
            
            if (debugLog) Debug.Log("[Sync - Fim]");
        };
    }

    public System.Action<object> WrapActionObject(System.Action action) {
        if (action == null) return null;
        return (obj) => {
            action.Invoke();
        };
    }

    public System.Action<object> WrapActionObject(System.Action<int> action, bool debugLog = false) {
        if (action == null) return null;
        return (obj) => {
            if (debugLog) Debug.Log("[Sync - Inicio]");

            if (obj is int valor) action.Invoke(valor);
            else Debug.LogError("O objeto passado não é um inteiro.");

            if (debugLog) Debug.Log("[Sync - Fim]");
        };
    }

    public System.Action<object> WrapActionObject(System.Action<GameObject> action, bool debugLog = false) {
        if (action == null) return null;
        return (obj) => {
            if (debugLog) Debug.Log("[Sync - Inicio]");

            if (obj is GameObject gameObject) action.Invoke(gameObject);
            else Debug.LogError("O objeto passado não é um GameObject.");

            if (debugLog) Debug.Log("[Sync - Fim]");
        };
    }


    #endregion
    
}
