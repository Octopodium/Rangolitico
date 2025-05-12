using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;

/*
    COMO UTILIZAR A SINCRONIZAÇÃO POR ATRIBUTO:

    A sincronização por atributo sincroniza chamada de métodos entre os clientes.
    Para utilizar, basta adicionar o atributo [Sincronizar("NOME_UNICO_AQUI")] no método que você quer sincronizar (método deve ser público).
    O método pode ou não receber parâmetros, porém só pode receber **UM** parâmetro. Confira os parâmetros suportados em Sincronizador.tiposSuportados.
    Além disso, deve chamar a extensão gameObject.Sincronizar("NOME_UNICO_AQUI", parametro) no inicio do método, para que o método seja chamado no outro cliente.
    (não irá chamar o método no cliente que chamou, apenas nos outros clientes)

    Para que isto funcione, o objeto que possui o método deve ter o componente Sincronizavel.
    Além disso, todos os componentes que possuirem métodos sincronizados devem implementar a interface SincronizaMetodo. (ou não será reconhecido)

    Exemplo de uso:

    [Sincronizar("setar-vida")]
    public void SetarVida(int vida) {
        gameObject.Sincronizar("setar-vida", vida);
        this.vida = vida;
    }

    Na maioria dos casos você chamará Sincronizar no inicio do método, mas você pode chamar em qualquer lugar.
    Considere que onde você chamar Sincronizar, será quando o método será chamado no outro cliente.
    Exemplo de chamada adiada:

    [Sincronizar("carregar-jogador")]
    public void CarregarJogador(GameObject jogador) {
        Player player = jogador.GetComponent<Player>();
        if (player == null) return; // Se não for um player, não faz nada.

        gameObject.Sincronizar("carregar-jogador", jogador);
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

    public static string GetTriggerDeFato(GameObject obj, string triggerName) {
        if (obj == null) return null;

        SincronizaMetodo sincronizaMetodo = obj.GetComponent<SincronizaMetodo>();
        if (sincronizaMetodo == null) {
            Debug.LogWarning("Alerta no objeto [" + obj.name + "]. Para utilizar o atributo [Sincronizar], é necessário implementar a interface SincronizaMetodo (vazia por padrão) no script que a utilize, caso o contrário, não será sincronizado.");
        }

        Sincronizavel sincronizavel = obj.GetComponent<Sincronizavel>();
        if (sincronizavel == null) {
            Debug.LogError("Erro no objeto [" + obj.name + "]. Para utilizar a extensão [gameObject.Sincronizar], é necessário que o gameObject possua o componente Sincronizavel.");
            return null;
        }

        return sincronizavel.GetTriggerDeFato(triggerName);
    }
    

    public static void Sincronizar(this GameObject obj, string triggerName) {
        if (obj == null) return;
    
        Sincronizador.instance.SetTrigger(GetTriggerDeFato(obj, triggerName));
    }

    public static void Sincronizar(this GameObject obj, string triggerName, int valor) {
        if (obj == null) return;

        Sincronizador.instance.SetTrigger(GetTriggerDeFato(obj, triggerName), valor);
    }

    public static void Sincronizar(this GameObject obj, string triggerName, GameObject valor) {
        if (obj == null) return;

        Sincronizador.instance.SetTrigger(GetTriggerDeFato(obj, triggerName), valor);
    }
}


[System.AttributeUsage(System.AttributeTargets.Method)]
public class SincronizarAttribute : System.Attribute {
    public string triggerName;
    public bool debugLog = false;
    public SincronizarAttribute(string triggerName, bool debugLog = false) {
        this.triggerName = triggerName;
        this.debugLog = debugLog;
    }
}


/// <summary>
/// Componente essencial para objetos que podem ser sincronizados.
/// Qualquer objeto que utilize a interface SincronizaMetodo e o atributo SincronizarAttribute, precisa ter esse componente.
/// </summary>
public class Sincronizavel : MonoBehaviour {

    [SerializeField] private string id = "";
    public string idObjetoSincronizado {
        get { return id; }
        set {
            if (string.IsNullOrEmpty(value)) return;

            if (jaCadastrado) {
                DescadastrarSincronizavel();
                DescadastrarMetodos();
            }

            id = value;

            if (jaCadastrado) {
                CadastrarSincronizavel();
                CadastrarMetodos();
            }
        }
    }

    private bool jaCadastrado = false;

    [Tooltip("Caso o objeto possua métodos sincronizados, eles serão cadastrados no Awake. Se desativado, deve ser chamado manualmente pelo LateSetup.")]
    public bool cadastrarNoInicio = true;
    private bool cadastrouUmaVez = false;


    [Tooltip("Caso positivo, inclui o nome da sala no ID. Deve ser utilizado em todos os objetos que não são mantidos entre as salas (a maioria).")]
    public bool exclusivoDaSala = true;

    private List<(string, System.Action<object>)> metodosSincronizados = new List<(string, System.Action<object>)>();
    private List<(string, System.Action)> metodosSincronizadosSemParametro = new List<(string, System.Action)>();
    private List<(Component, MethodInfo)> metodos = new List<(Component, MethodInfo)>();

    void Awake() {
        AcharAtributo();

        if (Sincronizador.instance == null) Sincronizador.onInstanciaCriada += Setup;
        else Setup();
    }

    void OnDestroy() {
        DescadastrarSincronizavel();
        DescadastrarMetodos();
    }

    void Setup() {
        if (cadastrarNoInicio && !cadastrouUmaVez) {
            CadastrarSincronizavel();
            CadastrarMetodos();
        }
    }

    void CadastrarSincronizavel() {
        if (idObjetoSincronizado == null || idObjetoSincronizado == "") idObjetoSincronizado = gameObject.name;

        idObjetoSincronizado = Sincronizador.instance.CertificarIDSincronizavel(idObjetoSincronizado, GetPrefixo(), GetSufixo());
        Sincronizador.instance.CadastrarSincronizavel(this);
    }

    void DescadastrarSincronizavel() {
        Sincronizador.instance.DescadastrarSincronizavel(this);
    }

    public void LateSetup() {
        if (jaCadastrado) return;

        CadastrarSincronizavel();
        CadastrarMetodos();
    }

    public string GetTriggerDeFato(string trigger) {
        return trigger + "_" + GetID();
    }

    private void AcharAtributo() {
        SincronizaMetodo[] componentes = GetComponents<SincronizaMetodo>();

        metodos.Clear();

        foreach (SincronizaMetodo componente in componentes) {
            var metodosNoComponente = componente.GetType().GetMethods();
            foreach (var metodo in metodosNoComponente) {
                var atributos = metodo.GetCustomAttributes(typeof(SincronizarAttribute), false);
                if (atributos.Length > 0) {
                    var atributo = (SincronizarAttribute)atributos[0];
                    if (atributo != null) {
                        metodos.Add(((Component) componente, metodo));
                    }
                }
            }
        }
    }

    
    private void CadastrarMetodos() {
        foreach (var (componente, metodo) in metodos) {
            var atributos = metodo.GetCustomAttributes(typeof(SincronizarAttribute), false);
            if (atributos.Length == 0 || atributos[0] == null) continue;
            SincronizarAttribute atributo = (SincronizarAttribute)atributos[0];
            
            bool debugLog = atributo.debugLog;
            string trigger = atributo.triggerName;
            string triggerDeFato = GetTriggerDeFato(trigger);
            if (string.IsNullOrEmpty(trigger)) {
                Debug.LogError("O método " + metodo.Name + " não possui um trigger definido. Não é possível sincronizar.");
                continue;
            }

            var parametros = metodo.GetParameters();
            if (parametros.Length > 1) {
                Debug.LogError("O método " + metodo.Name + " tem mais de um parâmetro. Não é possível sincronizar.");
                continue;
            }

            System.Action callbackSemParametro = null;
            System.Action<object> callback = null;

            if (parametros.Length == 0) {
                callbackSemParametro = Sincronizador.instance.WrapActionObjectSemParametro((System.Action) metodo.CreateDelegate(typeof(System.Action), componente), debugLog);
            } else if (parametros[0].ParameterType == typeof(GameObject)) {
                callback = Sincronizador.instance.WrapActionObject((System.Action<GameObject>) metodo.CreateDelegate(typeof(System.Action<GameObject>), componente), debugLog);
            } else if (parametros[0].ParameterType == typeof(int)) {
                callback = Sincronizador.instance.WrapActionObject((System.Action<int>) metodo.CreateDelegate(typeof(System.Action<int>), componente), debugLog);
            } else {
                string tiposSuportados = "";
                foreach (var tipo in Sincronizador.tiposSuportados) {
                    tiposSuportados += tipo.Name + ", ";
                }

                Debug.LogError("O método " + metodo.Name + " tem um tipo de parâmetro inválido para sincronização. Os tipos suportados são: " + tiposSuportados);
            }

            if (callback != null) {
                Sincronizador.instance.OnTrigger(triggerDeFato, callback);
                metodosSincronizados.Add((triggerDeFato, callback));
            } else if (callbackSemParametro != null) {
                Sincronizador.instance.OnTrigger(triggerDeFato, callbackSemParametro);
                metodosSincronizadosSemParametro.Add((triggerDeFato, callbackSemParametro));
            }
        }

        cadastrouUmaVez = true;
        jaCadastrado = true;
    }

    private void DescadastrarMetodos() {
        foreach (var (trigger, callback) in metodosSincronizados) {
            Sincronizador.instance.OffTrigger(trigger, callback);
        }

        foreach (var (trigger, callback) in metodosSincronizadosSemParametro) {
            Sincronizador.instance.OffTrigger(trigger, callback);
        }

        metodosSincronizados.Clear();
        metodosSincronizadosSemParametro.Clear();
        jaCadastrado = false;
    }


    private string GetSalaSufix() {
        if (!exclusivoDaSala) return "";
        return SceneManager.GetActiveScene().name;
    }

    public virtual string GetID() {
        return GetPrefixo() + idObjetoSincronizado + GetSufixo();
    }

    public virtual string GetSufixo() {
        return "";
    }

    public virtual string GetPrefixo() {
        string sufixo = GetSalaSufix();
        if (string.IsNullOrEmpty(sufixo)) return "";
        return GetSalaSufix() + "_";
    }
}
