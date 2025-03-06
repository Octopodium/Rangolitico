using UnityEngine;

public interface Interacao {
    public abstract void Interagir(Player jogador);
}

public interface InteracaoCondicional : Interacao {
    public abstract bool PodeInteragir(Player jogador);
}
