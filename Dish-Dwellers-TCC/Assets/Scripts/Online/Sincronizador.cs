using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;


public class Sincronizador : NetworkBehaviour {
    public static Sincronizador instance { get; private set; }
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
    public struct SincronizarBoolTriggerMessage : NetworkMessage {
        public string trigger;
        public bool valor;

        public SincronizarBoolTriggerMessage(string trigger, bool valor) {
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
    }




    public void SetTrigger(string triggerName) {
        if (isOnCallback) return;

        NetworkClient.Send(new SincronizarTriggerMessage(triggerName));
    }

    public void SetTrigger(string triggerName, int valor) {
        if (isOnCallback) return;

        NetworkClient.Send(new SincronizarIntTriggerMessage(triggerName, valor));
    }

    void ForeachConnection(System.Action<NetworkConnectionToClient> action, NetworkConnectionToClient except = null) {
        foreach (var conn in NetworkServer.connections) {
            NetworkConnectionToClient conexao = conn.Value;
            if (conexao == null || conexao == except) continue;
            action.Invoke(conexao);
        }
    }

    [Server]
    private void ServerOnSetTrigger(NetworkConnectionToClient quemChamou, SincronizarTriggerMessage triggerMessage) {
        string triggerName = triggerMessage.trigger;
        ForeachConnection((conexao) => {
            TargetSetTrigger(conexao, triggerName);
        }, quemChamou);
    }

    [TargetRpc]
    private void TargetSetTrigger(NetworkConnectionToClient target, string triggerName) {
        isOnCallback = true;

        if (triggers.ContainsKey(triggerName)) {
            foreach (var action in triggers[triggerName]) {
                action.Invoke();
            }
        }

        isOnCallback = false;
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
        isOnCallback = true;

        if (triggersComParametro.ContainsKey(triggerName)) {
            foreach (var action in triggersComParametro[triggerName]) {
                action.Invoke(valor);
            }
        }

        isOnCallback = false;
    }


    

    public void OnTrigger(string triggerName, System.Action action) {
        if (!triggers.ContainsKey(triggerName)) {
            triggers[triggerName] = new List<System.Action>();
        }
        triggers[triggerName].Add(action);
    }

    public void OnTrigger(string triggerName, System.Action<object> action) {
        if (!triggersComParametro.ContainsKey(triggerName)) {
            triggersComParametro[triggerName] = new List<System.Action<object>>();
        }

        triggersComParametro[triggerName].Add(action);
    }

    public void OffTrigger(string triggerName, System.Action action) {
        if (triggers.ContainsKey(triggerName)) {
            triggers[triggerName].Remove(action);
        }
    }

    public void OffTrigger(string triggerName, System.Action<object> action) {
        if (triggersComParametro.ContainsKey(triggerName)) {
            triggersComParametro[triggerName].Remove(action);
        }
    }
}
