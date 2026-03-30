using UnityEngine;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using System;
using System.Linq;

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
    public bool debugLogChamadas = false;

    private void Awake() {
        if (instance == null) {
            instance = this;

            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);

        } else {
            Destroy(gameObject);
            return;
        }

        System.Action acoes = onInstanciaCriada;
        onInstanciaCriada = null;

        acoes?.Invoke();
    }

    public override void OnStartClient () {
        transform.SetParent(GameManager.instance.transform);
        onInstanciaCriada?.Invoke();
        onInstanciaCriada = null;
    }
    

    void OnDestroy() {
        instance = null;
        onInstanciaCriada = null;
    }


    #region Utils

    public Player GetPlayerOfConnection(NetworkConnectionToClient connection) {
        if (GameManager.instance.jogadores == null) return null;

        foreach (Player player in GameManager.instance.jogadores) {
            if (player.GetComponent<NetworkIdentity>()?.connectionToClient == connection)
                return player;
        }

        return null;
    }

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
            Debug.LogWarning("Sincronizavel de ID [" + id + "] não foi cadastrado pois já havia um com o mesmo ID! Ele é: " + sincronizaveis[id]?.name, obj.gameObject);
            return false;
        }

        if (debugLogSincronizaveis) {
            Debug.Log("Cadastrando sincronizável de " + obj.name + " ID[" + id + "]", obj.gameObject);
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
            Debug.Log("Descadastrando sincronizável de " + obj.name + " ID[" + id + "]. Encontrou e removeu: " + existe, obj.gameObject);
        }
    }

    public Sincronizavel GetSincronizavel(string id) {
        if (sincronizaveis.ContainsKey(id)) {
            return sincronizaveis[id];
        } else {
            return null;
        }
    }

    public SubSincronizavel GetSubSincronizavel(string identificador) {
        string[] partes = identificador.Split("***SUB_", 2, StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length < 2) return null;

        string sinc_id = partes[0];
        int sub = int.Parse(partes[1]);

        Sincronizavel sincronizavel = GetSincronizavel(sinc_id);
        if (sincronizavel == null) return null;
        return sincronizavel.GetSub(sub);
    }

    #endregion

    #region Sincronizar Metodos

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

        GameObject pai = info.componenteDoMetodo?.gameObject;

        if (debugLogMetodos) {
            Debug.Log("Cadastrando método [" + info.GetNome(id) + "]", pai);
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
        GameObject pai = info.componenteDoMetodo?.gameObject;

        if (info.opcoes != null) {
            if (info.opcoes.unico) {
                DescadastrarMetodo(info, id);
            }

            if (info.opcoes.cooldown > 0) {
                if (metodosOnCooldown.Contains(nome)) {
                    Debug.LogWarning("Método [" + nome + "] está em cooldown!", pai);
                    return false;
                } else {
                    SetMetodoOnCooldown(nome, info.opcoes.cooldown);
                }
            }

            if (!info.opcoes.repeteParametro && ParametrosRepetidos(nome, parametros)) {
                Debug.LogWarning("Método [" + nome + "] não será chamado com os mesmos parâmetros!", pai);
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

        if (debugLogChamadas) {
            Debug.Log($"Chamando método [{nomeMetodo}] com os valores: {string.Join(", ", valores)}");
        }

        //currentTriggerOnCallback.Add(nomeMetodo);
        isOnCallbackCall = true;
        foreach (InformacoesMetodo info in listaMetodos) {
            if (info.componenteDoMetodo == null) {
                Debug.LogError("O componente do método [" + info.GetNome() + "] não existe mais!");
                continue;
            }

            GameObject pai = info.componenteDoMetodo.gameObject;

            if (info.metodo == null) {
                Debug.LogError("O método [" + info.GetNome() + "] não existe mais!", pai);
                continue;
            }

            /*try {*/
            if (info.opcoes != null && info.opcoes.debug) Debug.Log("Chamando método [" + info.GetNome() + "] com os valores: " + string.Join(", ", valores), pai);
            info.metodo.Invoke(info.componenteDoMetodo, valores);
            if (info.opcoes != null && info.opcoes.debug) Debug.Log("Método [" + info.GetNome() + "] chamado com sucesso!", pai);
            /*} catch (System.Exception e) {
                string valoresString = string.Join(',', valores);
                Debug.LogError("Erro ao chamar o método [" + info.GetNome() + "] com os valores [" + valoresString + "]: " + e.Message, pai);
            }*/
        }
        isOnCallbackCall = false;
        //currentTriggerOnCallback.Remove(nomeMetodo);
    }

    #endregion

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

    #region Spawn
    [Serializable]
    public class SpawnInfo {
        public string id;
        public Vector3 position;
        public Action<GameObject> callback;
        public List<string> ids;

        public SpawnInfo(string id, Vector3 position, System.Action<GameObject> callback) {
            this.id = id;
            this.position = position;
            this.callback = callback;
            this.ids = new List<string>();
        }

        public bool AddId(string id) {
            if (ids.Contains(id)) return false;
            ids.Add(id);
            return true;
        }

        public bool FreeId() {
            if (ids.Count == 0) return false;

            ids.RemoveAt(0);
            return true;
        }

        public bool TemId() {
            return ids.Count > 0;
        }
    }

    public bool InstanciarNetworkObject(GameObject prefab, Sincronizavel sinc, Vector3 pos, Quaternion rotation = default) {
        if (!GameManager.instance.isOnline) return false;
        

        NetworkIdentity netId = prefab.GetComponent<NetworkIdentity>();
        if (netId == null) {
            Debug.LogError("O prefab [" + prefab.name + "] não possui um NetworkIdentity. Não é possível instanciar objetos de rede sem este componente.");
            return false;
        }

        if (isServer) {
            GameObject objeto = Instantiate(prefab, pos, rotation);
            NetworkServer.Spawn(objeto);
            AposInstanciado(objeto, sinc.GetID());
        }

        return true;
    }

    [ClientRpc]
    public void AposInstanciado(GameObject objeto, string sincId) {
        Sincronizavel sinc = GetSincronizavel(sincId);
        sinc?.HandleObjetoSpawnado(objeto);
    }


    SpawnInfo GetSpawnerMaisProvavel(Sincronizavel sinc, List<SpawnInfo> list) {
        if (list.Count == 1) return list[0];

        float minDist = float.MaxValue;
        List<SpawnInfo> possiveis_pos = new List<SpawnInfo>();

        foreach (SpawnInfo s_info in list) {
            float dist = Vector3.Distance(s_info.position, sinc.posInicial);

            if (dist <= minDist) {
                if (dist < minDist) {
                    minDist = dist;
                    possiveis_pos.Clear();
                }
                possiveis_pos.Add(s_info);
            }
        }

        if (possiveis_pos.Count == 1) return possiveis_pos[0];

        // Se ainda sim, ainda não há criterio de desempate. Pega o primeiro :/
        return possiveis_pos[0];
    }

    #endregion

    #region Trocar personagem
    // Aqui já é gambiarra
    public void TrocarPersonagens() {
        if (isServer)
            TrocarPersonagensCmd();
    }

    [Command(requiresAuthority = false)]
    public void TrocarPersonagensCmd() {
        Player anglerPlayer = GameManager.instance.GetPlayer(QualPersonagem.Angler);
        Player heaterPlayer = GameManager.instance.GetPlayer(QualPersonagem.Heater);

        NetworkConnectionToClient anglerConn = anglerPlayer.connectionToClient;
        NetworkConnectionToClient heaterConn = heaterPlayer.connectionToClient;

        NetworkServer.RemovePlayerForConnection(anglerConn, RemovePlayerOptions.KeepActive);
        NetworkServer.RemovePlayerForConnection(heaterConn, RemovePlayerOptions.KeepActive);

        anglerPlayer.netIdentity.AssignClientAuthority(heaterConn);
        heaterPlayer.netIdentity.AssignClientAuthority(anglerConn);

        NetworkServer.AddPlayerForConnection(anglerConn, heaterPlayer.gameObject);
        NetworkServer.AddPlayerForConnection(heaterConn, anglerPlayer.gameObject);

        ForeachConnection((conexao) => {
            RpcAtualizarPlayerPosTroca(conexao);
        });
    }

    [TargetRpc]
    public void RpcAtualizarPlayerPosTroca(NetworkConnectionToClient _conn) {
        foreach (Player p in GameManager.instance.jogadores) {
            p.OnTrocouAutoridade();
        }
    }

    #endregion

}