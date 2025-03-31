using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour{

    // Informações da câmera : 
    ModoDeJogo modoDeJogoConfigurado = ModoDeJogo.INDEFINIDO;
    public bool ativo = true; // Substitui o modo "INATIVO" previamente implementado.
    [SerializeField] private CinemachineCamera[] cameras = new CinemachineCamera[2];

    const float distance = 10, height = 40, FOV = 75;

    private void Awake(){
    }

    private void Start(){
        DeterminaModoDeCamera();
        ConfigurarCameras();
    }

    private void DeterminaModoDeCamera(){
        if (!ativo) return;

        modoDeJogoConfigurado = GameManager.instance.modoDeJogo;

        switch(modoDeJogoConfigurado) {
            case ModoDeJogo.SINGLEPLAYER:
                GameManager.instance.OnTrocarControle += TrocarCamera;
                break;

            case ModoDeJogo.MULTIPLAYER_LOCAL:
                GameManager.instance.GetPlayerInput(QualPlayer.Player1)["Move"].performed += TrocarCamera1;
                GameManager.instance.GetPlayerInput(QualPlayer.Player2)["Move"].performed += TrocarCamera2;
                break;

            case ModoDeJogo.MULTIPLAYER_ONLINE:
                GameManager.instance.OnTrocarControle += TrocarCamera;
                TrocarCamera(GameManager.instance.playerOnlineAtual);
                break;
        }

    }

    void OnDisable(){
        switch(modoDeJogoConfigurado) {
            case ModoDeJogo.SINGLEPLAYER:
                GameManager.instance.OnTrocarControle -= TrocarCamera;
                break;

            case ModoDeJogo.MULTIPLAYER_LOCAL:
                GameManager.instance.GetPlayerInput(QualPlayer.Player1)["Move"].performed -= TrocarCamera1;
                GameManager.instance.GetPlayerInput(QualPlayer.Player2)["Move"].performed -= TrocarCamera2;
                break;
            
            case ModoDeJogo.MULTIPLAYER_ONLINE:
                GameManager.instance.OnTrocarControle -= TrocarCamera;
                break;
        }
    }


    #region Configuração inicial

    private void ConfigurarCameras(){
        List<Player> players = GameManager.instance.jogadores;

        for (int i = 0; i < players.Count; i++){
            cameras[i].Follow = players[i].transform;
        }
    }

    // Alterna entre cameras.
    public void TrocarCamera(QualPlayer player){
        Debug.Log("Pedido de trocar camera para " + player.ToString());
        if (player == QualPlayer.Player1) TrocarCamera1();
        else TrocarCamera2();
    }

    public void TrocarCamera1(){
        cameras[0].Priority = 1;
        cameras[1].Priority = 0;
    }

    void TrocarCamera1(InputAction.CallbackContext ctx){
        TrocarCamera1();
    }

    public void TrocarCamera2(){
        cameras[1].Priority = 1;
        cameras[0].Priority = 0;
    }

    void TrocarCamera2(InputAction.CallbackContext ctx){
        TrocarCamera2();
    }

    #endregion

}
