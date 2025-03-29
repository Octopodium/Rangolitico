using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyInfo {
    public static LobbyInfo instance;
    public enum Modo { Host, Entrar };
    public Modo modo = Modo.Host;
}

public class EscolherEntrarLobbyUI : MonoBehaviour {
    public static EscolherEntrarLobbyUI instance;

    public string cenaLobby = "Lobby";
    public string cenaPrimeiraFase = "1-1";
    public GameObject menuOpcoes, lobbyOpcoes;
    


    void Awake() {
        instance = this;

        
    }

    public void JogarOffline() {
        if (GameManager.instance != null)
            GameManager.instance.isOnline = false;

        SceneManager.LoadScene(cenaPrimeiraFase, LoadSceneMode.Single);
    }

    public void Hostear() {
        LobbyInfo.instance = new LobbyInfo();
        LobbyInfo.instance.modo = LobbyInfo.Modo.Host;
        SceneManager.LoadScene(cenaLobby, LoadSceneMode.Single);
    }

    public void Entrar() {
        LobbyInfo.instance = new LobbyInfo();
        LobbyInfo.instance.modo = LobbyInfo.Modo.Entrar;
        SceneManager.LoadScene(cenaLobby, LoadSceneMode.Single);
    }

    public void MostrarOpcoes() {
        menuOpcoes.SetActive(false);
        lobbyOpcoes.SetActive(true);
    }

    public void Voltar() {
        menuOpcoes.SetActive(true);
        lobbyOpcoes.SetActive(false);
    }
}
