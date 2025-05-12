using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;


public class SincronizadorVazio : NetworkBehaviour, SincronizadorComTipo {
    Sincronizador sinc;

    public struct SincronizarTriggerMessage : NetworkMessage {
        public string trigger;

        public SincronizarTriggerMessage(string trigger) {
            this.trigger = trigger;
        }
    }

    public System.Type GetTipo() {
        return null;
    }

    public void Setup(Sincronizador sinc) {
        this.sinc = sinc;
        NetworkServer.RegisterHandler<SincronizarTriggerMessage>(ServerOnSetTrigger);
    }

    
    public void SetTrigger(string triggerName, object valor) {
        SetTrigger(triggerName);
    }

    public void SetTrigger(string triggerName) {
        if (!sinc.CanSetTrigger(triggerName)) return;

        NetworkClient.Send(new SincronizarTriggerMessage(triggerName));
    }

    [Server]
    public void ServerOnSetTrigger(NetworkConnectionToClient quemChamou, SincronizarTriggerMessage triggerMessage) {
        string triggerName = triggerMessage.trigger;
        sinc.ForeachConnection((conexao) => {
            TargetSetTrigger(conexao, triggerName);
        }, quemChamou);
    }

    [TargetRpc]
    public void TargetSetTrigger(NetworkConnectionToClient target, string triggerName) {
        sinc.ForeachTriggerSemParametro(triggerName, (action) => {
            action.Invoke();
        });
    }
    
}
