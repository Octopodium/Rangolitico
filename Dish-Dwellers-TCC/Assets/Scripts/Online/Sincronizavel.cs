using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

/*
    COMO UTILIZAR A SINCRONIZAÇÃO POR ATRIBUTO:

    A sincronização por atributo sincroniza chamada de métodos entre os clientes.
    Para utilizar, basta adicionar o atributo [Sincronizar("NOME_UNICO_AQUI")] no método que você quer sincronizar.
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

        Sincronizavel sincronizavel = obj.GetComponent<Sincronizavel>();
        if (sincronizavel == null) {
            Debug.LogError("Erro no objeto [" + obj.name + "]. Para utilizar a extensão [gameObject.Sincronizar], é necessário que o gameObject possua o componente Sincronizavel.");
            return null;
        }

        return triggerName + "_" + sincronizavel.GetID();
    }
    

    public static void Sincronizar(this GameObject obj, string triggerName) {
        if (obj == null) return;
    
        Sincronizador.instance.SetTrigger(GetTriggerDeFato(obj, triggerName));
    }

    public static void Sincronizar(this GameObject obj, string triggerName, int valor) {
        if (obj == null) return;

        Sincronizador.instance.SetTrigger(GetTriggerDeFato(obj, triggerName), valor);
        Debug.Log("Sincronizando " + obj.name + " com o trigger " + triggerName + " e valor " + valor);
    }

    public static void Sincronizar(this GameObject obj, string triggerName, GameObject valor) {
        if (obj == null) return;

        Sincronizador.instance.SetTrigger(GetTriggerDeFato(obj, triggerName), valor);
        Debug.Log("Sincronizando " + obj.name + " com o trigger " + triggerName + " e valor " + valor.name);
    }
}


[System.AttributeUsage(System.AttributeTargets.Method)]
public class SincronizarAttribute : System.Attribute {
    public string triggerName;
    public SincronizarAttribute(string triggerName) {
        this.triggerName = triggerName;
    }
}




/// <summary>
/// Componente essencial para objetos que podem ser sincronizados.
/// Qualquer objeto que utilize a interface SincronizaMetodo e o atributo SincronizarAttribute, precisa ter esse componente.
/// </summary>
public class Sincronizavel : MonoBehaviour {
    public string idObjetoSincronizado;
    private List<(string, System.Action<object>)> metodosSincronizados = new List<(string, System.Action<object>)>();

    void Awake() {
        if (Sincronizador.instance == null) Sincronizador.onInstanciaCriada += Setup;
        else Setup();
    }

    void Setup() {
        Sincronizador.instance.CadastrarSincronizavel(this);
        AcharAtributo();
    }

    void OnDestroy() {
        Sincronizador.instance.DescadastrarSincronizavel(this);
        DescadastrarMetodos();
    }

    private void AcharAtributo() {
        SincronizaMetodo[] componentes = GetComponents<SincronizaMetodo>();
        List<(Component, MethodInfo)> metodosEncontrados = new List<(Component, MethodInfo)>();

        foreach (SincronizaMetodo componente in componentes) {
            var metodos = componente.GetType().GetMethods();
            foreach (var metodo in metodos) {
                var atributos = metodo.GetCustomAttributes(typeof(SincronizarAttribute), false);
                if (atributos.Length > 0) {
                    var atributo = (SincronizarAttribute)atributos[0];
                    if (atributo != null) {
                        metodosEncontrados.Add(((Component) componente, metodo));
                    }
                }
            }
        }

        CadastrarMetodos(metodosEncontrados);
    }

    private void CadastrarMetodos(List<(Component, MethodInfo)> metodos) {
        foreach (var (componente, metodo) in metodos) {
            var atributos = metodo.GetCustomAttributes(typeof(SincronizarAttribute), false);
            if (atributos.Length == 0 || atributos[0] == null) continue;
            SincronizarAttribute atributo = (SincronizarAttribute)atributos[0];

            string trigger = atributo.triggerName;
            string triggerDeFato = trigger + "_" + GetID();
            if (string.IsNullOrEmpty(trigger)) {
                Debug.LogError("O método " + metodo.Name + " não possui um trigger definido. Não é possível sincronizar.");
                continue;
            }

            var parametros = metodo.GetParameters();
            if (parametros.Length > 1) {
                Debug.LogError("O método " + metodo.Name + " tem mais de um parâmetro. Não é possível sincronizar.");
                continue;
            }

            System.Action<object> callback = null;

            if (parametros.Length == 0) {
                callback = Sincronizador.instance.WrapActionObject((System.Action) metodo.CreateDelegate(typeof(System.Action), componente));
            } else if (parametros[0].ParameterType == typeof(GameObject)) {
                callback = Sincronizador.instance.WrapActionObject((System.Action<GameObject>) metodo.CreateDelegate(typeof(System.Action<GameObject>), componente));
            } else if (parametros[0].ParameterType == typeof(int)) {
                callback = Sincronizador.instance.WrapActionObject((System.Action<int>) metodo.CreateDelegate(typeof(System.Action<int>), componente));
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
            }
        }
    }

    private void DescadastrarMetodos() {
        foreach (var (trigger, callback) in metodosSincronizados) {
            Sincronizador.instance.OffTrigger(trigger, callback);
        }
        metodosSincronizados.Clear();
    }

    public virtual string GetID() {
        return idObjetoSincronizado;
    }
}
