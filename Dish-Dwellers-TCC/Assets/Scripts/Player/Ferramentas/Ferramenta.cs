public interface Ferramenta {

    /// <summary>
    /// Deve ser chamada antes de qualquer ação da ferramenta
    /// </summary>
    void Inicializar(Player jogador);

    /// <summary>
    /// Chamada quando o jogador aperta o botão de ferramenta
    /// </summary>
    void Acionar();

    /// <summary>
    /// Chamada quando o jogador solta o botão de ferramenta
    /// </summary>
    void Soltar();
}
