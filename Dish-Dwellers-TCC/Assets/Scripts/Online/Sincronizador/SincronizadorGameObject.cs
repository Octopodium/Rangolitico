using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;


public class SincronizadorGameObject : NetworkBehaviour, SincronizadorComTipo {
    Sincronizador sinc;

    public struct SincronizarStringTriggerMessage : NetworkMessage {
        public string trigger;
        public string valor;

        public SincronizarStringTriggerMessage(string trigger, string valor) {
            this.trigger = trigger;
            this.valor = valor;
        }
    }

    public System.Type GetTipo() {
        return typeof(GameObject);
    }

    public void Setup(Sincronizador sinc) {
        this.sinc = sinc;
        NetworkServer.RegisterHandler<SincronizarStringTriggerMessage>(ServerOnSetTrigger);
    }

    
    public void SetTrigger(string triggerName, object valor) {
        GameObject val = (GameObject) valor;
        SetTrigger(triggerName, val);
    }

    public void SetTrigger(string triggerName, GameObject obj) {
        if (!sinc.CanSetTrigger(triggerName)) return;

        Sincronizavel sincronizavel = obj.GetComponent<Sincronizavel>();
        if (sincronizavel == null || sincronizavel.GetID().Trim() == "") {
            Debug.LogError("Para sincronizar um parâmetro <GameObject>, é necessário que este possua o componente <Sincronizavel> com um id único.");
            return;
        }

        string id = sincronizavel.GetID();
        NetworkClient.Send(new SincronizarStringTriggerMessage(triggerName, id));
    }

    [Server]
    private void ServerOnSetTrigger(NetworkConnectionToClient quemChamou, SincronizarStringTriggerMessage triggerMessage) {
        string triggerName = triggerMessage.trigger;

        sinc.ForeachConnection((conexao) => {
            TargetSetTrigger(conexao, triggerName, triggerMessage.valor);
        }, quemChamou);
    }

    [TargetRpc]
    private void TargetSetTrigger(NetworkConnectionToClient target, string triggerName, string valor) {
        Sincronizavel sincronizavel = sinc.GetSincronizavel(valor);
        if (sincronizavel != null) {
            GameObject obj = sincronizavel.gameObject;
            sinc.ForeachTriggerComParametro(triggerName, (action) => {
                action.Invoke(obj);
            });
        }
    }
}
