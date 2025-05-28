public interface Ferramenta {
    bool acionada { get; }

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

    /// <summary>
    /// Chamada quando a ferramenta foi acionado mas não foi possível completar a ação, por exemplo, quando o jogador mira o gancho e desiste.
    /// </summary>
    void Cancelar();
}
