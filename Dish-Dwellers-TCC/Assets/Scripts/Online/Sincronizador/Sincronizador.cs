using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/*
    EI, VOCÊ AÍ!
    Se você estiver utilizando o atributo [Sincronizar], é melhor você olhar o arquivo Sincronizavel.cs
    Este código é a versão no pelo, só pra quem sabe o que está fazendo.

    "Ai mas eu não sei nenhum dos dois", ok, então vai pro outro!
    "Mas eu não quero, eu quero fazer no pelo!", vai por sua conta e risco então camarada.
*/

public class OpcoesDeExecucaoDeMetodo {
    public bool debug = false; // Debugar a chamada
    public bool unico = false; // Se o método só pode ser chamado uma vez
    public float cooldown = -1; // Tempo de espera entre chamadas do método
    public bool repeteParametro = true; // Se o método pode ser chamado com os mesmos parâmetros da ultima vez
}


public struct InformacoesMetodo {
    public MethodInfo metodo;
    public Component componenteDoMetodo;
    public OpcoesDeExecucaoDeMetodo opcoes;

    public string GetNome(string id = "") {
        return componenteDoMetodo.GetType().Name + "." + metodo.Name + (id != "" ? "_" + id : "");
    }

    public bool IsValid => metodo != null && componenteDoMetodo != null;
}


public class Sincronizador : NetworkBehaviour {
    public static Sincronizador instance { get; private set; }
    public static System.Action onInstanciaCriada;


    protected Dictionary<string, Sincronizavel> sincronizaveis = new Dictionary<string, Sincronizavel>();
    protected Dictionary<string, HashSet<InformacoesMetodo>> metodos = new Dictionary<string, HashSet<InformacoesMetodo>>();
    protected HashSet<string> metodosOnCooldown = new HashSet<string>();
    protected Dictionary<string, object[]> parametrosUltimosChamados = new Dictionary<string, object[]>();
    protected HashSet<string> currentTriggerOnCallback = new HashSet<string>();

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
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

    protected void SetMetodoOnCooldown(string id, float tempo) {
        if (metodosOnCooldown.Contains(id)) return;

        metodosOnCooldown.Add(id);
        StartCoroutine(RemoverCooldown(id, tempo));
    }

    IEnumerator RemoverCooldown(string id, float tempo) {
        yield return new WaitForSeconds(tempo);
        metodosOnCooldown.Remove(id);
    }

    protected bool ParametrosRepetidos(string id, object[] parametros) {
        if (parametrosUltimosChamados.ContainsKey(id)) {
            object[] ultimosChamados = parametrosUltimosChamados[id];
            parametrosUltimosChamados[id] = parametros;

            if (ultimosChamados.Length != parametros.Length) return false;

            for (int i = 0; i < ultimosChamados.Length; i++) {
                if (!ultimosChamados[i].Equals(parametros[i])) return false;
            }

            return true;
        }

        parametrosUltimosChamados[id] = parametros;
        return false;
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
        while (sincronizaveis.ContainsKey(prefixo + idValidado + sufixo)) {
            idValidado = id + "_" + i;
            i++;
        }

        return idValidado;
    }

    public bool CadastrarSincronizavel(Sincronizavel obj) {
        string idOriginal = obj.GetID();
        string id = idOriginal;

        if (sincronizaveis.ContainsKey(id)) {
            Debug.LogWarning("Sincronizavel de ID [" + id + "] não foi cadastrado pois já havia um com o mesmo ID!");
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


    public InformacoesMetodo CadastrarMetodo(MethodInfo metodo, Component componenteDoMetodo, string id = "") {
        InformacoesMetodo info = new InformacoesMetodo();
        info.metodo = metodo;
        info.componenteDoMetodo = componenteDoMetodo;

        return CadastrarMetodo(info, id);
    }

    public InformacoesMetodo CadastrarMetodo(InformacoesMetodo info, string id = "") {
        string nome = info.GetNome(id);

        if (!CanSetOnTrigger(nome)) return new InformacoesMetodo();

        if (metodos.ContainsKey(nome)) {
            if (!metodos[nome].Contains(info)) {
                metodos[nome].Add(info);
            }
        } else {
            HashSet<InformacoesMetodo> lista = new HashSet<InformacoesMetodo>();
            lista.Add(info);
            metodos[nome] = lista;
        }

        return info;
    }

    public void DescadastrarMetodo(MethodInfo metodo, Component componenteDoMetodo, string id = "") {
        InformacoesMetodo info = new InformacoesMetodo();
        info.metodo = metodo;
        info.componenteDoMetodo = componenteDoMetodo;

        DescadastrarMetodo(info, id);
    }

    public void DescadastrarMetodo(InformacoesMetodo info, string id = "") {
        string nome = info.GetNome(id);
        if (!CanUnsetOnTrigger(nome)) return;

        if (metodos[nome].Contains(info)) {
            metodos[nome].Remove(info);
        }
    }

    /// <summary>
    /// Chama o método em outro cliente.
    /// </summary>
    /// <returns>Retorna true se o método que o chamou pode prosseguir com o funcionamento. Retorna false apenas se houver alguma restrinção;</returns>
    public bool ChamarMetodo(MethodInfo metodo, Component componenteDoMetodo, object[] parametros = null, string id = "") {
        InformacoesMetodo info = new InformacoesMetodo { metodo = metodo, componenteDoMetodo = componenteDoMetodo };
        return ChamarMetodo(info, parametros, id);
    }

    /// <summary>
    /// Chama o método em outro cliente.
    /// </summary>
    /// <returns>Retorna true se o método que o chamou pode prosseguir com o funcionamento. Retorna false apenas se houver alguma restrinção;</returns>
    public bool ChamarMetodo(InformacoesMetodo info, object[] parametros = null, string id = "") {
        string nome = info.GetNome(id);

        if (info.opcoes != null) {
            if (info.opcoes.unico) {
                DescadastrarMetodo(info, id);
            }

            if (info.opcoes.cooldown > 0) {
                if (metodosOnCooldown.Contains(nome)) {
                    Debug.LogWarning("Método [" + nome + "] está em cooldown!");
                    return false;
                } else {
                    SetMetodoOnCooldown(nome, info.opcoes.cooldown);
                }
            }

            if (!info.opcoes.repeteParametro && ParametrosRepetidos(nome, parametros)) {
                Debug.LogWarning("Método [" + nome + "] não será chamado com os mesmos parâmetros!");
                return false;
            }
        }

        if (!CanSetTrigger(nome)) return true; // Pode executar o método, só não pode chamar o trigger

        if (parametros == null) parametros = new object[0];

        ValorGenerico[] valores = new ValorGenerico[parametros.Length];
        for (int i = 0; i < parametros.Length; i++) {
            valores[i] = new ValorGenerico(parametros[i]);
        }

        CmdChamarMetodo(nome, valores);
        return true;
    }

    [Command(requiresAuthority = false)]
    public void CmdChamarMetodo(string nomeMetodo, ValorGenerico[] v, NetworkConnectionToClient sender = null) {
        ForeachConnection((conexao) => {
            RpcChamarMetodo(conexao, nomeMetodo, v);
        }, sender);
    }

    [TargetRpc]
    public void RpcChamarMetodo(NetworkConnectionToClient conn, string nomeMetodo, ValorGenerico[] v) {
        var listaMetodos = metodos[nomeMetodo];
        if (listaMetodos == null) return;

        object[] valores = new object[v.Length];
        for (int i = 0; i < v.Length; i++) {
            valores[i] = v[i].valor;
        }

        currentTriggerOnCallback.Add(nomeMetodo);
        foreach (InformacoesMetodo info in listaMetodos) {
            if (info.componenteDoMetodo == null) {
                Debug.LogError("O componente do método [" + info.GetNome() + "] não existe mais!");
                continue;
            }

            if (info.metodo == null) {
                Debug.LogError("O método [" + info.GetNome() + "] não existe mais!");
                continue;
            }

            try {
                if (info.opcoes != null && info.opcoes.debug) Debug.Log("Chamando método [" + info.GetNome() + "] com os valores: " + string.Join(", ", valores));
                info.metodo.Invoke(info.componenteDoMetodo, valores);
                if (info.opcoes != null && info.opcoes.debug) Debug.Log("Método [" + info.GetNome() + "] chamado com sucesso!");
            } catch (System.Exception e) {
                Debug.LogError("Erro ao chamar o método [" + info.GetNome() + "]: " + e.Message);
            }
        }
        currentTriggerOnCallback.Remove(nomeMetodo);
    }


    #region Set e Unset de Evento de Trigger

    public bool IsOnline() {
        return GameManager.instance != null && GameManager.instance.isOnline;
    }

    // Se pode triggar
    public bool CanSetTrigger(string triggerName) {
        return IsOnline() && triggerName != null && !currentTriggerOnCallback.Contains(triggerName) && metodos.ContainsKey(triggerName);
    }

    // Se pode cadastrar
    public bool CanSetOnTrigger(string triggerName) {
        return IsOnline() && triggerName != null;
    }

    // Se pode cadastrar
    public bool CanUnsetOnTrigger(string triggerName) {
        return IsOnline() && triggerName != null && metodos.ContainsKey(triggerName);
    }

    #endregion

}
