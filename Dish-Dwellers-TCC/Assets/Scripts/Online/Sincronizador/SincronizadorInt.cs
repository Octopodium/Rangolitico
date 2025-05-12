using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;


public class SincronizadorInt : NetworkBehaviour, SincronizadorComTipo {
    Sincronizador sinc;

    public struct SincronizarIntTriggerMessage : NetworkMessage {
        public string trigger;
        public int valor;

        public SincronizarIntTriggerMessage(string trigger, int valor) {
            this.trigger = trigger;
            this.valor = valor;
        }
    }

    public System.Type GetTipo() {
        return typeof(int);
    }

    public void Setup(Sincronizador sinc) {
        this.sinc = sinc;
        NetworkServer.RegisterHandler<SincronizarIntTriggerMessage>(ServerOnSetTrigger);
    }

    
    public void SetTrigger(string triggerName, object valor) {
        int val = (int) valor;
        SetTrigger(triggerName, val);
    }


    public void SetTrigger(string triggerName, int valor) {
        if (!sinc.CanSetTrigger(triggerName)) return;

        NetworkClient.Send(new SincronizarIntTriggerMessage(triggerName, valor));
    }

    [Server]
    private void ServerOnSetTrigger(NetworkConnectionToClient quemChamou, SincronizarIntTriggerMessage triggerMessage) {
        string triggerName = triggerMessage.trigger;

        sinc.ForeachConnection((conexao) => {
            TargetSetTrigger(conexao, triggerName, triggerMessage.valor);
        }, quemChamou);
    }

    [TargetRpc]
    private void TargetSetTrigger(NetworkConnectionToClient target, string triggerName, int valor) {
        sinc.ForeachTriggerComParametro(triggerName, (action) => {
            action.Invoke(valor);
        });
    }
    
}
