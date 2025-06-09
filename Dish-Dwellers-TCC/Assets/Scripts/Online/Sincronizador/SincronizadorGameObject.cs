using UnityEngine;

public partial struct ValorGenerico {
    public string CodificadorCustomGameObject(GameObject obj) {
        if (obj == null) return "";

        Sincronizavel sincronizavel = obj.GetComponent<Sincronizavel>();
        if (sincronizavel == null || sincronizavel.GetID().Trim() == "") {
            Debug.LogError("Para sincronizar um parâmetro <GameObject>, é necessário que este possua o componente <Sincronizavel> com um id único.");
            return "";
        }

        return sincronizavel.GetID();
    }

    public ValorGenerico DecodificadorCustomGameObject(string id) {
        Sincronizavel sincronizavel = Sincronizador.instance.GetSincronizavel(id);
        if (sincronizavel != null) {
            return new ValorGenerico(typeof(GameObject), sincronizavel.gameObject);
        } else {
            Debug.LogError("Sincronizavel não encontrado com ID: " + id);
            return new ValorGenerico();
        }
    }
}