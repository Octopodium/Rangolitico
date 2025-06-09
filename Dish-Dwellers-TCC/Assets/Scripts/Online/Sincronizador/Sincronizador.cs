using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;

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
    protected bool isOnCallbackCall = false;

    [Header("Debug")]
    public bool debugLogMetodos = false;
    public bool debugLogSincronizaveis = false;

    private void Awake() {
        if (instance == null) {
            instance = this;

            if (transform.parent == null)
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

    public bool CadastrarSincronizavel(Sincronizavel obj) {
        string idOriginal = obj.GetID();
        string id = idOriginal;

        if (sincronizaveis.ContainsKey(id)) {
            Debug.LogWarning("Sincronizavel de ID [" + id + "] não foi cadastrado pois já havia um com o mesmo ID! Ele é: " + sincronizaveis[id]?.name);
            return false;
        }

        if (debugLogSincronizaveis) {
            Debug.Log("Cadastrando sincronizável de " + obj.name + " ID[" + id + "]");
        }

        sincronizaveis[id] = obj;
        return true;
    }

    public void DescadastrarSincronizavel(Sincronizavel obj) {
        string id = obj.GetID();

        bool existe = sincronizaveis.ContainsKey(id) && sincronizaveis[id] == obj;

        if (existe)
            sincronizaveis.Remove(id);

        if (debugLogSincronizaveis) {
            Debug.Log("Descadastrando sincronizável de " + obj.name + " ID[" + id + "]. Encontrou e removeu: " + existe);
        }
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

        if (debugLogMetodos) {
            Debug.Log("Cadastrando método [" + info.GetNome(id) + "]");
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

    bool sincronizacaoTravada = false;

    public void TravarSincronizacao(System.Action callback) {
        sincronizacaoTravada = true;
        callback?.Invoke();
        sincronizacaoTravada = false;
    }

    public void TravarSincronizacao(IEnumerator callback) {
        StartCoroutine(TravarSincronizacaoCoroutine(callback));
    }

    IEnumerator TravarSincronizacaoCoroutine(IEnumerator callback) {
        sincronizacaoTravada = true;
        yield return callback;
        sincronizacaoTravada = false;
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
        if (sincronizacaoTravada) return false;

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
        if (!metodos.ContainsKey(nomeMetodo)) {
            Debug.LogError("Método [" + nomeMetodo + "] não encontrado!");
            return;
        }

        var listaMetodos = metodos[nomeMetodo];
        if (listaMetodos == null) return;

        object[] valores = new object[v.Length];
        for (int i = 0; i < v.Length; i++) {
            valores[i] = v[i].valor;
        }

        //currentTriggerOnCallback.Add(nomeMetodo);
        isOnCallbackCall = true;
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
                string valoresString = string.Join(',', valores);
                Debug.LogError("Erro ao chamar o método [" + info.GetNome() + "] com os valores [" + valoresString + "]: " + e.Message);
            }
        }
        isOnCallbackCall = false;
        //currentTriggerOnCallback.Remove(nomeMetodo);
    }


    #region Set e Unset de Evento de Trigger

    public bool IsOnline() {
        return GameManager.instance != null && GameManager.instance.isOnline;
    }

    // Se pode triggar
    public bool CanSetTrigger(string triggerName) {
        return IsOnline() && triggerName != null && !isOnCallbackCall /*!currentTriggerOnCallback.Contains(triggerName)*/ && metodos.ContainsKey(triggerName);
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

    Dictionary<uint, System.Action<GameObject>> spawnHandlers = new Dictionary<uint, System.Action<GameObject>>();
    public void InstanciarNetworkObject(System.Action<GameObject> callback, GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null, bool unico = false) {
        if (!GameManager.instance.isOnline) return;

        NetworkIdentity netId = prefab.GetComponent<NetworkIdentity>();
        if (netId == null) {
            Debug.LogError("O prefab [" + prefab.name + "] não possui um NetworkIdentity. Não é possível instanciar objetos de rede sem este componente.");
            return;
        }

        uint id = netId.assetId;

        if (unico && GetSpawnedObject(id) != null) {
            Debug.LogWarning($"[Sincronizador] Já existe um objeto com AssetId: {id}. Não será instanciado novamente.");
            return;
        }

        bool isSpawning = spawnHandlers.ContainsKey(id);

        if (isSpawning) {
            spawnHandlers[id] = callback;
        } else {
            spawnHandlers.Add(id, callback);
        }

        if (NetworkClient.localPlayer.isServer && (!unico || !isSpawning)) {
            Debug.Log($"[Sincronizador] Instanciando objeto de rede com AssetId: {id}.");
            GameObject objeto = Instantiate(prefab, position, rotation, parent);
            NetworkServer.Spawn(objeto);
        }

    }

    /// <summary>
    /// Quando um Sincronizavel é instanciado possuindo um NetworkIdentity, chama este método para verificar se a instanciação foi através do InstanciarNetworkObject.
    /// Se sim, chama o callback associado ao assetId do NetworkIdentity.
    /// </summary>
    /// <param name="netId"></param>
    public void CheckSeAguardandoSpawn(NetworkIdentity netId) {
        uint id = netId.assetId;
        if (spawnHandlers.ContainsKey(id)) {
            Debug.Log($"[Sincronizador] Encontrou o objeto spawnado com NetId: {netId.netId} e AssetId: {id}.");
            GameObject objeto = netId.gameObject;
            System.Action<GameObject> handler = spawnHandlers[id];
            spawnHandlers.Remove(id);
            handler?.Invoke(objeto);
        }
    }

    /// <summary>
    /// Se está no aguardo de spawn do objeto porém este foi destruído, libera o aguardo.
    /// </summary>
    /// <param name="netId"></param>
    public void LiberarAguardoSpawn(NetworkIdentity netId) {
        uint id = netId.assetId;
        if (spawnHandlers.ContainsKey(id)) {
            spawnHandlers.Remove(id);
        }
    }

    public bool IsSpawning(uint assetId) {
        return spawnHandlers.ContainsKey(assetId);
    }

    public bool IsSpawning(NetworkIdentity netId) {
        return IsSpawning(netId.assetId);
    }

    public bool IsSpawning(GameObject obj) {
        NetworkIdentity netId = obj.GetComponent<NetworkIdentity>();
        if (netId == null) return false;
        return IsSpawning(netId);
    }

    public GameObject GetSpawnedObject(uint assetId) {
        NetworkIdentity[] allObjects = FindObjectsByType<NetworkIdentity>(FindObjectsSortMode.None);
        foreach (NetworkIdentity netId in allObjects) {
            if (netId.assetId == assetId) {
                Sincronizavel sincronizavel = netId.GetComponent<Sincronizavel>();
                
                if (sincronizavel != null) {
                    return sincronizavel.isDestroying ? null : netId.gameObject;
                }

                return netId.gameObject;
            }
        }
        return null;
    }

}