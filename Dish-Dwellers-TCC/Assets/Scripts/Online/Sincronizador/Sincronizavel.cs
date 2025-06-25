using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using Mirror;
using System;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

/*
    COMO UTILIZAR A SINCRONIZAÇÃO POR ATRIBUTO:

    A sincronização por atributo sincroniza chamada de métodos entre os clientes.
    Para utilizar, basta adicionar o atributo [Sincronizar] no método que você quer sincronizar (método deve ser público).
    O método pode ou não receber parâmetros, até no máximo 5 parâmetros (cheque os tipos suportados pelo Mirror ou pelo seu projeto).
    Além disso, deve chamar a extensão gameObject.Sincronizar(parametro1, parametro2, ...) no inicio do método, para que o método seja chamado no outro cliente.
    (não irá chamar o método no cliente que chamou, apenas nos outros clientes)

    Para que isto funcione, o objeto que possui o método deve ter o componente Sincronizavel.
    Além disso, todos os componentes que possuirem métodos sincronizados devem implementar a interface SincronizaMetodo. (ou não será reconhecido)

    Exemplo de uso:

    [Sincronizar]
    public void SetarVida(int vida) {
        gameObject.Sincronizar(vida);
        this.vida = vida;
    }

    Na maioria dos casos você chamará Sincronizar no inicio do método, mas você pode chamar em qualquer lugar.
    Considere que onde você chamar Sincronizar, será quando o método será chamado no outro cliente.
    Exemplo de chamada adiada:

    [Sincronizar]
    public void CarregarJogador(GameObject jogador) {
        Player player = jogador.GetComponent<Player>();
        if (player == null) return; // Se não for um player, não faz nada.

        gameObject.Sincronizar(jogador);
        // ... código para carregar o jogador ...
    }

    Neste caso, se o objeto não possuir o componente Player, não tem porque chamar este mesmo método no outro cliente.


    EXPLICAÇÃO IMPORTANTE:
    
    Entender o porque de utilizar esse tipo de sincronização é IMPORTANTE.
    O Mirror nem sempre sincroniza os elementos corretamente, fazendo com que algumas coisas sejam chamadas em um cliente e não no outro.
    Isso não é algo previsivel, depende da internet e outros fatores incontroláveis.
    O ato de sincronizar um método, garante que o mesmo será chamado em todos os clientes.
    Por isso, ao criar um método sincronizado, você tem que considerar quais variaveis podem estar diferentes entre os clientes.
    Não só isso, mas também que há MUITA chance do método ser chamado duas vezes no mesmo cliente (caso o Mirror tenha chamado o método corretamente).
    Para evitar problemas, tente trabalhar com valores completos ao invés de variações.
    Pense em qual é o vetor de sincronização, ao invés de pensar em qual é o valor que você quer sincronizar.
    Por exemplo, ao invés de sincronizar a vida do jogador, sincronize o que está dando o dano.
    
    Nosso projeto utiliza essa forma de sincronização pois o jogo também é jogado localmente, e o Mirror não é necessário, assim, não podemos ter uma dependência do mesmo.
    Quando o jogo está no modo offline, os métodos de sincronização são ignorados.
*/

/// <summary>
/// Interface necessária para utilizar o atributo Sincronizar. Também é necessário ter o componente Sincronizavel no objeto.
/// </summary>
public interface SincronizaMetodo {}

/// <summary>
/// Extensão utilizada para passar parametros de sincronização ao utilizar o atributo Sincronizar. Também é necessário ter o componente Sincronizavel no objeto.
/// </summary>
public static class SincronizavelExtensions {

    public static InformacoesMetodo GetMetodo(GameObject obj, string methodName, out string id) {
        id = "";
        if (obj == null) return new InformacoesMetodo();

        SincronizaMetodo sincronizaMetodo = obj.GetComponent<SincronizaMetodo>();
        if (sincronizaMetodo == null) {
            Debug.LogWarning("Alerta no objeto [" + obj.name + "]. Para utilizar o atributo [Sincronizar], é necessário implementar a interface SincronizaMetodo (vazia por padrão) no script que a utilize, caso o contrário, não será sincronizado.");
        }

        Sincronizavel sincronizavel = obj.GetComponent<Sincronizavel>();
        if (sincronizavel == null) {
            Debug.LogError("Erro no objeto [" + obj.name + "]. Para utilizar a extensão [gameObject.Sincronizar], é necessário que o gameObject possua o componente Sincronizavel.");
            return new InformacoesMetodo();
        }

        id = sincronizavel.GetID();
        return sincronizavel.GetMetodo(methodName);
    }

    public static bool Sincronizar(this GameObject obj, object[] parametros, [CallerMemberName] string triggerName = "") {
        if (obj == null) return false;
        if (!GameManager.instance.isOnline) return true;

        string id = "";
        InformacoesMetodo infos = GetMetodo(obj, triggerName, out id);

        if (infos.IsValid) return Sincronizador.instance.ChamarMetodo(infos, parametros, id);

        Debug.LogWarning("Método invalido: " + triggerName);
        return false;
    }

    // Lista de métodos de sincronização com 1 a 5 parâmetros. (não é possível utilizar "params object[]" por causa do [CallerMemberName])
    public static bool Sincronizar(this GameObject obj, object arg1 = null, object arg2 = null, object arg3 = null, object arg4 = null, object arg5 = null, [CallerMemberName] string triggerName = "") {
        if (!GameManager.instance.isOnline) return true;

        object[] args = new object[] { arg1, arg2, arg3, arg4, arg5 };
        List<object> argsList = new List<object>(args);

        foreach (var arg in args) {
            if (arg == null) argsList.Remove(arg);
        }

        return Sincronizar(obj, argsList.ToArray(), triggerName);
    }

    public static void NaoSincronizar(this GameObject obj, System.Action action) {
        if (obj == null) return;
        if (!GameManager.instance.isOnline) {
            action?.Invoke();
            return;
        }

        Sincronizador.instance.TravarSincronizacao(action);
    }
}


[System.AttributeUsage(System.AttributeTargets.Method)]
public class SincronizarAttribute : System.Attribute {
    public bool debugLog = false;
    public bool unico = false;
    public bool repeteParametro = true;
    public float cooldown = -1;
    public SincronizarAttribute(bool debugLog = false, bool unico = false, float cooldown = -1, bool repeteParametro = true) {
        this.debugLog = debugLog;
        this.unico = unico;
        this.cooldown = cooldown;
        this.repeteParametro = repeteParametro;
    }

    public OpcoesDeExecucaoDeMetodo GerarOpcoes() {
        OpcoesDeExecucaoDeMetodo opcoes = new OpcoesDeExecucaoDeMetodo();
        opcoes.debug = debugLog;
        opcoes.unico = unico;
        opcoes.cooldown = cooldown;
        opcoes.repeteParametro = repeteParametro;

        return opcoes;
    }
}


/// <summary>
/// Componente essencial para objetos que podem ser sincronizados.
/// Qualquer objeto que utilize a interface SincronizaMetodo e o atributo SincronizarAttribute, precisa ter esse componente.
/// </summary>
public class Sincronizavel : MonoBehaviour {
    protected Dictionary<string, InformacoesMetodo> metodosSincronizados = new Dictionary<string, InformacoesMetodo>();

    public bool debugLogMetodosCadastrados = false;

    private bool jaCadastrado = false;

    [Tooltip("Caso o objeto possua métodos sincronizados, eles serão cadastrados no Awake. Se desativado, deve ser chamado manualmente pelo LateSetup.")]
    public bool cadastrarNoInicio = true;
    private bool cadastrouUmaVez = false;
    public bool naoUsarIDAuto = false;
    public bool isSingleton = false;



    private List<(Component, MethodInfo)> metodos = new List<(Component, MethodInfo)>();
    NetworkIdentity networkIdentity = null;

    void Awake() {
        networkIdentity = GetComponent<NetworkIdentity>();
        if (networkIdentity != null) {
            StartCoroutine(EsperandoNetID());
            return;
        }

        if (Sincronizador.instance == null) Sincronizador.onInstanciaCriada += Setup;
        else Setup();
    }

    IEnumerator EsperandoNetID() {
        yield return new WaitUntil(() => networkIdentity != null && networkIdentity.netId != 0);
        
        Sincronizador.instance.CheckSeAguardandoSpawn(networkIdentity);

        identificador = networkIdentity.netId + "";
        naoUsarIDAuto = true;

        if (Sincronizador.instance == null) Sincronizador.onInstanciaCriada += Setup;
        else Setup();
    }

    void OnDestroy() {
        PreDestroy();
    }

    void Setup() {
        if (cadastrarNoInicio && !cadastrouUmaVez) {
            CadastrarSincronizavel();
            CadastrarMetodos();
        }
    }

    public void LateSetup() {
        if (jaCadastrado) return;

        CadastrarSincronizavel();
        CadastrarMetodos();
    }

    public bool isDestroying { get;  protected set; } = false;

    public void PreDestroy() {
        if (isDestroying) return;
        isDestroying = true;

        DescadastrarSincronizavel();
        DescadastrarMetodos();

        if (networkIdentity != null) {
            Sincronizador.instance.LiberarAguardoSpawn(networkIdentity);
        }
    }



#if UNITY_EDITOR

    public bool IsPrefab() {
        return EditorSceneManager.IsPreviewScene(gameObject.scene) || EditorUtility.IsPersistent(gameObject);
    }

    void OnValidate() {
        if (IsPrefab() && !isSingleton) {
            identificador = "";
            return;
        }

        if (identificador != null && !string.IsNullOrEmpty(identificador)) return;
        GeraID();
    }



    [ContextMenu("Re-gerar todos os IDS")]
    public void RegerarIDS() {
        Sincronizavel[] sincronizaveis = FindObjectsByType<Sincronizavel>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Sincronizavel sinc in sincronizaveis) {
            sinc.GeraID();
            EditorUtility.SetDirty(sinc);
        }
    }

#endif

    public string AlgoritmoDoID() {
        return "$" + Guid.NewGuid().ToString("N");
    }

    [HideInInspector]
    public string identificador = "";

    public void GeraID() {
        if (naoUsarIDAuto) return;
        identificador = "";

        Sincronizavel[] sincronizaveis = FindObjectsByType<Sincronizavel>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        while (identificador == "") {
            identificador = AlgoritmoDoID();

            bool achou = false;
            foreach (Sincronizavel sincronizavel in sincronizaveis) {
                if (sincronizavel == null || sincronizavel == this || sincronizavel.identificador == null || sincronizavel.identificador == "") continue;
                if (sincronizavel.identificador == identificador) {
                    achou = true;
                    break;
                }
            }

            if (achou) {
                identificador = "";
            }
        }

        Debug.Log("Gerando ID [" + identificador + "] para o objeto [" + gameObject.name + "]", gameObject);
    }


    #region Objeto Sincronizavel

    void CadastrarSincronizavel() {
        if (identificador == null || identificador == "") GeraID();
        Sincronizador.instance.CadastrarSincronizavel(this);
    }

    void DescadastrarSincronizavel() {
        if (Sincronizador.instance == null) return;
        Sincronizador.instance.DescadastrarSincronizavel(this);
    }

    public string GetTriggerDeFato(string trigger) {
        return trigger + "_" + GetID();
    }

    public virtual string GetID() {
        return GetPrefixo() + identificador + GetSufixo();
    }

    public virtual string GetSufixo() {
        return "";
    }

    public virtual string GetPrefixo() {
        return "";
    }

    #endregion


    #region Metodos Sincronizados
    private void CadastrarMetodos() {
        SincronizaMetodo[] componentes = GetComponents<SincronizaMetodo>();

        metodosSincronizados.Clear();

        foreach (SincronizaMetodo componente in componentes) {
            var metodosNoComponente = componente.GetType().GetMethods();
            foreach (MethodInfo metodo in metodosNoComponente) {
                var atributos = metodo.GetCustomAttributes(typeof(SincronizarAttribute), false);
                if (atributos.Length > 0) {
                    var atributo = (SincronizarAttribute)atributos[0];
                    if (atributo != null) {
                        if (debugLogMetodosCadastrados) Debug.Log("Cadastrando método [" + metodo.Name + "] no objeto [" + gameObject.name + "] de ID [" + identificador + "]");
                        CadastrarMetodo(metodo, (Component)componente, atributo.GerarOpcoes());
                    }
                }
            }
        }

        cadastrouUmaVez = true;
        jaCadastrado = true;
    }

    private void CadastrarMetodo(MethodInfo method, Component componente, OpcoesDeExecucaoDeMetodo opcoes = null) {
        InformacoesMetodo info = new InformacoesMetodo();
        info.metodo = method;
        info.componenteDoMetodo = componente;
        info.opcoes = opcoes;

        metodosSincronizados.Add(method.Name, info);
        Sincronizador.instance.CadastrarMetodo(info, GetID());
    }

    public InformacoesMetodo GetMetodo(string nome) {
        if (metodosSincronizados.ContainsKey(nome)) {
            return metodosSincronizados[nome];
        } else {
            return new InformacoesMetodo();
        }
    }

    private void DescadastrarMetodos() {
        foreach (var metodo in metodosSincronizados) {
            Sincronizador.instance.DescadastrarMetodo(metodo.Value, GetID());
        }

        metodosSincronizados.Clear();
    }

    #endregion
}