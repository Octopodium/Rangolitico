using UnityEngine;

// Serve apenas como referencia a um Sincronizavel, não tendo utilidade prática nem operando como um Sincronizavel de verdade
public class SubSincronizavel : MonoBehaviour {
    public Sincronizavel dono;
    public int id;
    public bool autoCadastro = false;

    public void Start() {
        if (autoCadastro) dono.AddSub(this);
    }

    public string GetIdentificador() {
        return dono.identificador + "***SUB_"  +id;
    }
}
