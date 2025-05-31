using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DevTools : MonoBehaviour {
    [Header("Trocar de cena")]
    public Transform trocarCenaPanel;
    public Dropdown cenasDropdown;
    bool isInDevTools = false;


    void Awake() {
        GerarDrowpdownCenas();

    }


    void Start() {
        GameManager.instance.input.Geral.PreDevTools.performed += ctx => isInDevTools = true;
        GameManager.instance.input.Geral.PreDevTools.canceled += ctx => isInDevTools = false;

        GameManager.instance.input.Geral.DevTools1.performed += ctx => { if (isInDevTools) ToggleTrocarCenaPanel(); };
        GameManager.instance.input.Geral.DevTools2.performed += ctx => { if (isInDevTools) ResetarSala(); };
        GameManager.instance.input.Geral.DevTools3.performed += ctx => { if (isInDevTools) VoltarMenu(); };
        GameManager.instance.input.Geral.DevTools4.performed += ctx => { if (isInDevTools) ToggleEstadoPorta(); };
        GameManager.instance.input.Geral.DevTools5.performed += ctx => { if (isInDevTools) TeleportaAnglerParaHeater(); };
        GameManager.instance.input.Geral.DevTools6.performed += ctx => { if (isInDevTools) TeleportaHeaterParaAngler(); };
    }

    // Shift + 1 - Trocar de cena
    #region Trocar de Cena

    public void IrParaCena(string nomeCena) {
        GameManager.instance.ForcarCenaAguardando();
        SceneManager.LoadScene(nomeCena);
    }

    protected virtual void GerarDrowpdownCenas() {
        cenasDropdown.ClearOptions();

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            cenasDropdown.options.Add(new Dropdown.OptionData(sceneName));
        }
    }

    public void OnIrCenaSelecionada() {
        int selectedIndex = cenasDropdown.value;
        string nome = cenasDropdown.options[selectedIndex].text;

        IrParaCena(nome);
    }

    public void IrParaProximaCena() {
        GameManager.instance.ForcarCenaAguardando();
    }

    public void ToggleTrocarCenaPanel() {
        trocarCenaPanel.gameObject.SetActive(!trocarCenaPanel.gameObject.activeSelf);
    }

    #endregion

    // Shift + 2 - Reseta a sala atual
    public void ResetarSala() {
        GameManager.instance.ResetSala();
    }

    // Shift + 3 - Volta para o menu principal
    public void VoltarMenu() {
        GameManager.instance.VoltarParaMenu();
    }

    // Shift + 4 - Alterna o estado da porta
    public void ToggleEstadoPorta() {
        Porta porta = FindFirstObjectByType<Porta>();
        if (porta == null) {
            Debug.LogWarning("Nenhuma porta encontrada na cena.");
            return;
        }

        if (porta.trancada) {
            porta.Destrancar();
        } else {
            porta.Trancar();
        }
    }


    // Shift + 5 e Shift + 6 (Teleporte Angler e Heater)
    #region Teleporte

    public void Teleportransporta(Player recebe, Player vai) {
        Vector3 posicao = recebe.transform.position + recebe.direcao;
        vai.Teletransportar(posicao);
    }

    public (Player angler, Player heater) ObterAnglerHeater() {
        Player angler = null;
        Player heater = null;

        foreach (Player player in GameManager.instance.jogadores) {
            if (player.personagem == QualPersonagem.Angler) {
                angler = player;
            } else if (player.personagem == QualPersonagem.Heater) {
                heater = player;
            }
        }


        return (angler, heater);
    }


    // Shift + 5 - Teleporta Angler para Heater
    public void TeleportaAnglerParaHeater() {
        var (angler, heater) = ObterAnglerHeater();
        if (angler == null || heater == null) {
            Debug.LogWarning("Angler ou Heater não encontrado.");
            return;
        }

        Teleportransporta(angler, heater);
    }

    // Shift + 6 - Teleporta Heater para Angler
    public void TeleportaHeaterParaAngler() {
        var (angler, heater) = ObterAnglerHeater();
        if (angler == null || heater == null) {
            Debug.LogWarning("Angler ou Heater não encontrado.");
            return;
        }

        Teleportransporta(heater, angler);
    }


    #endregion

}
