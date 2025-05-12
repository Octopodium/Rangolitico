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


public interface SincronizadorComTipo {
    public void Setup(Sincronizador sinc);
    public System.Type GetTipo();
    public void SetTrigger(string triggerName, object valor);
}


public class Sincronizador : NetworkBehaviour {
    public static Sincronizador instance { get; private set; }
    public static System.Action onInstanciaCriada;

    public static System.Type[] tiposSuportados = new System.Type[] { typeof(int), typeof(GameObject) };

    Dictionary<System.Type, SincronizadorComTipo> sincronizadores = new Dictionary<System.Type, SincronizadorComTipo>();
    SincronizadorComTipo sincronizadorSemParametro = null;


    protected Dictionary<string, Sincronizavel> sincronizaveis = new Dictionary<string, Sincronizavel>();
    protected Dictionary<string, List<System.Action>> triggers = new Dictionary<string, List<System.Action>>();
    protected Dictionary<string, List<System.Action<object>>> triggersComParametro = new Dictionary<string, List<System.Action<object>>>();

    bool isOnCallback = false;

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }

        SincronizadorComTipo[] sincronizadores = GetComponentsInChildren<SincronizadorComTipo>();
        foreach (var sincronizador in sincronizadores) {
            if (sincronizador == null) continue;
            System.Type tipo = sincronizador.GetTipo();

            if (tipo == null && sincronizadorSemParametro == null) {
                sincronizadorSemParametro = sincronizador;
                sincronizadorSemParametro.Setup(this);
            } else if (!this.sincronizadores.ContainsKey(tipo)) {
                this.sincronizadores[tipo] = sincronizador;
                sincronizador.Setup(this);
            }
        }
  
        onInstanciaCriada?.Invoke();
        onInstanciaCriada = null;
    }


    #region Utils

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

    public void ForeachTriggerSemParametro(string triggerName, System.Action<System.Action> action) {
        isOnCallback = true;

        if (triggersComParametro.ContainsKey(triggerName)) {
            foreach (var acao in triggers[triggerName]) {
                action.Invoke(acao);
            }
        }

        isOnCallback = false;
    }

    #endregion



    #region Cadastro de Sincronizaveis

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

    #endregion



    #region Set e Unset de Evento de Trigger

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
    
    // Sincronização de triggers com parametro 
    public void SetTrigger<T>(string triggerName, T value) {
        System.Type tipo = typeof(T);
        if (this.sincronizadores.ContainsKey(tipo)) {
            this.sincronizadores[tipo].SetTrigger(triggerName, value);
        }
    }

    public void SetTrigger(string triggerName) {
        if (this.sincronizadorSemParametro != null) {
            this.sincronizadorSemParametro.SetTrigger(triggerName, null);
        }
    }
    

    public System.Action WrapActionObjectSemParametro(System.Action action, bool debugLog = false) {
        if (action == null) return null;
        return () => {
            if (debugLog) Debug.Log("[Sync - Inicio]");

            action.Invoke();
            
            if (debugLog) Debug.Log("[Sync - Fim]");
        };
    }

    public System.Action<object> WrapActionObject<T>(System.Action<T> action, bool debugLog = false) {
        if (action == null) return null;
        return (obj) => {
            if (debugLog) Debug.Log("[Sync - Inicio]");

            if (obj is T valor) action.Invoke(valor);
            else Debug.LogError("O objeto passado não é um " + nameof(T) + ".");

            if (debugLog) Debug.Log("[Sync - Fim]");
        };
    }
    
}
