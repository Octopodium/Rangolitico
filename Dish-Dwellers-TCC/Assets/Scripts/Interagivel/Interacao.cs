using UnityEngine;

public interface Interacao {
    public abstract void Interagir();
}

public interface InteracaoCondicional : Interacao {
    public abstract bool PodeInteragir();
}
