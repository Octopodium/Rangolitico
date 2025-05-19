using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System.Reflection;

public partial struct ValorGenerico {
    public System.Type tipo;
    public object valor;

    public bool IsValid => tipo != null && valor != null;

    public ValorGenerico(System.Type tipo, object valor) {
        this.tipo = tipo;
        this.valor = valor;
    }

    public ValorGenerico(object valor) {
        this.tipo = valor.GetType();
        this.valor = valor;
    }

    public static ValorGenerico Decodificar(string codificado) {
        ValorGenerico decodificadorCustomizado = ValorGenericoReaderWriter.GetDecodificadorCustomizado(codificado);
        if (decodificadorCustomizado.IsValid) return decodificadorCustomizado;

        string[] partes = codificado.Split('|');

        System.Type tipo = System.Type.GetType(partes[0]);
        object valor = null;

        if (tipo == null) {
            Debug.LogError("Tipo não encontrado: " + partes[0]);
        } else {
            valor = System.Convert.ChangeType(partes[1], tipo);
        }

        return new ValorGenerico(tipo, valor);
    }

    public string Codificar() {
        string codificadorCustomizado = ValorGenericoReaderWriter.GetCodificadorCustomizado(this);
        return this.tipo.AssemblyQualifiedName + "|" + (codificadorCustomizado ?? this.valor.ToString());
    }

    public override string ToString() {
        return valor?.ToString() ?? "null";
    }
}

public static class ValorGenericoReaderWriter {
    public static void WriteValorGenerico(this NetworkWriter writer, ValorGenerico valor) {
        writer.WriteString(valor.Codificar());
    }

    public static ValorGenerico ReadeValorGenerico(this NetworkReader reader) {
        return ValorGenerico.Decodificar(reader.ReadString());
    }


    #region Codificadores Customizados

    /*
        Como adicionar um codificador customizado:
        1. Veja o codificador de GameObject como exemplo
        2. Crie uma partial struct de ValorGenerico
        3. Adicione o método de codificação com o nome "CodificadorCustom<TIPO>", ele deve receber o tipo como parâmetro e retornar uma string
        4. Adicione o método de decodificação com o nome "DecodificadorCustom<TIPO>", ele deve receber uma string como parâmetro e retornar um ValorGenerico
    */

    private static bool codificadoresCustomizadosCadastrados = false;

    private struct Codificador {
        public MethodInfo codificador;
        public MethodInfo decodificador;
        public bool IsValid => codificador != null && decodificador != null;
    }

    private static Dictionary<System.Type, Codificador> codificadores = new Dictionary<System.Type, Codificador>();

    public static void RegisterCodificador(System.Type tipo, MethodInfo codificador, MethodInfo decodificador) {
        if (codificadores.ContainsKey(tipo)) {
            Debug.LogError("Codificador já registrado para o tipo: " + tipo);
            return;
        }

        Debug.Log("Registrando codificador para o tipo: " + tipo);

        codificadores.Add(tipo, new Codificador {
            codificador = codificador,
            decodificador = decodificador
        });
    }

    private static void RegisterCodificadoresCustomizados() {
        if (codificadoresCustomizadosCadastrados) return;


        var metodos = typeof(ValorGenerico).GetMethods();
        foreach (MethodInfo metodo in metodos) {
            if (metodo.Name.StartsWith("CodificadorCustom")) {
                var parametros = metodo.GetParameters();
                if (parametros.Length != 1) continue;

                System.Type tipo = parametros[0].ParameterType;

                // Checa validade: o tipo do parâmetro já está registrado?
                if (codificadores.ContainsKey(tipo)) {
                    Debug.LogError("Codificador já registrado para o tipo: " + tipo);
                    continue;
                }

                // Checa validade: codificador retorna string?
                if (metodo.ReturnType != typeof(string)) {
                    Debug.LogError("Codificador inválido para o tipo: " + tipo);
                    continue;
                }

                string sufixo = metodo.Name.Substring("CodificadorCustom".Length);
                var decodificador = typeof(ValorGenerico).GetMethod("DecodificadorCustom" + sufixo);

                // Checa validade: decodificador existe?
                if (decodificador == null) {
                    Debug.LogError("Decodificador não encontrado para o tipo: " + tipo);
                    continue;
                }

                // Checa validade: decodificador tem 1 parâmetro do tipo string?
                if (decodificador.GetParameters().Length != 1 || decodificador.GetParameters()[0].ParameterType != typeof(string)) {
                    Debug.LogError("Decodificador inválido para o tipo: " + tipo);
                    continue;
                }

                // Checa validade: decodificador retorna ValorGenerico?
                if (decodificador.ReturnType != typeof(ValorGenerico)) {
                    Debug.LogError("Método de codificação ou decodificação inválido para o tipo: " + tipo);
                    continue;
                }

                // Se tudo estiver certo, registra o codificador e decodificador
                codificadores.Add(tipo, new Codificador {
                    codificador = metodo,
                    decodificador = decodificador
                });
            }
        }

        codificadoresCustomizadosCadastrados = true;
    }

    public static string GetCodificadorCustomizado(ValorGenerico valorGenerico) {
        if (!codificadoresCustomizadosCadastrados) RegisterCodificadoresCustomizados();

        Codificador metodo = codificadores.ContainsKey(valorGenerico.tipo) ? codificadores[valorGenerico.tipo] : new Codificador();

        if (metodo.IsValid) {
            object resultado = metodo.codificador.Invoke(valorGenerico, new object[] { valorGenerico.valor });
            return resultado?.ToString() ?? null;
        }


        return null;
    }

    public static ValorGenerico GetDecodificadorCustomizado(string valorCodificado) {
        if (!codificadoresCustomizadosCadastrados) RegisterCodificadoresCustomizados();

        string[] partes = valorCodificado.Split('|');
        System.Type tipo = System.Type.GetType(partes[0]);
        if (tipo == null) return new ValorGenerico();


        if (codificadores.ContainsKey(tipo)) {
            ValorGenerico valorGenerico = new ValorGenerico(tipo, null);
            Codificador metodo = codificadores[tipo];
            return (ValorGenerico) metodo.decodificador.Invoke(valorGenerico, new object[] { partes[1] });
        }

        return new ValorGenerico();
    }

    #endregion
}
