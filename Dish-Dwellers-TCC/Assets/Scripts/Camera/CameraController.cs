using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour{

    // Informações da câmera : 
    ModoDeJogo modoDeJogoConfigurado = ModoDeJogo.INDEFINIDO;
    public bool ativo = true; // Substitui o modo "INATIVO" previamente implementado.
    [SerializeField] private CinemachineCamera[] cameras = new CinemachineCamera[2];
    [SerializeField] private CinemachineCamera introCamera;
    private float tempoDBlendNormal = 0.5f, tempoDBlendIntro = 2.0f;
    private bool podeTrocarCamera = false;


    private void Start(){
        if(introCamera != null) FazerIntroducao();
        else podeTrocarCamera = true;

        DeterminaModoDeCamera();
        ConfigurarCameras();
    }

    private void DeterminaModoDeCamera(){
        if (!ativo) return;

        modoDeJogoConfigurado = GameManager.instance.modoDeJogo;

        switch(modoDeJogoConfigurado) {
            case ModoDeJogo.SINGLEPLAYER:
                GameManager.instance.OnTrocarControle += TrocarCamera;
                TrocarCamera(GameManager.instance.playerAtual);
                break;

            case ModoDeJogo.MULTIPLAYER_LOCAL:
                GameManager.instance.GetPlayerInput(QualPlayer.Player1)["Move"].performed += TrocarCamera1;
                GameManager.instance.GetPlayerInput(QualPlayer.Player2)["Move"].performed += TrocarCamera2;
                break;

            case ModoDeJogo.MULTIPLAYER_ONLINE:
                TrocarCamera1();
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
                break;
        }
    }

    #region Camera de introdução

    private void FazerIntroducao(){
        StartCoroutine(Introducao());
    }

    IEnumerator Introducao(){
        CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
        brain.DefaultBlend.Time = tempoDBlendIntro;

        introCamera.Priority = 2;
        yield return new WaitForSeconds(1.0f);

        introCamera.Priority = 0;
        cameras[0].Priority = 1;

        yield return new WaitForSeconds(2.1f);
        brain.DefaultBlend.Time = tempoDBlendNormal;

        podeTrocarCamera = true;
    }

    #endregion

    #region Configuração inicial

    private void ConfigurarCameras(){
        List<Player> players = GameManager.instance.jogadores;

        if (modoDeJogoConfigurado == ModoDeJogo.MULTIPLAYER_ONLINE) {
            Player jogador = players[0].isLocalPlayer ? players[0] : players[1];
            Player outro_jogador = players[0].isLocalPlayer ? players[1] : players[0];

            cameras[0].Follow = jogador.transform;
            cameras[1].Follow = outro_jogador.transform;

            return;
        }

        for (int i = 0; i < players.Count; i++){
            if (players[i].qualPlayer == QualPlayer.Player1) cameras[0].Follow = players[i].transform;
            else cameras[1].Follow = players[i].transform;
        }
    }

    // Alterna entre cameras.
    public void TrocarCamera(QualPlayer player){
        if (player == QualPlayer.Player1) TrocarCamera1();
        else TrocarCamera2();
    }

    public void TrocarCamera1(){
        if(!podeTrocarCamera) return;
        cameras[0].Priority = 1;
        cameras[1].Priority = 0;
    }

    void TrocarCamera1(InputAction.CallbackContext ctx){
        TrocarCamera1();
    }

    public void TrocarCamera2(){
        if(!podeTrocarCamera) return;
        cameras[1].Priority = 1;
        cameras[0].Priority = 0;
    }

    void TrocarCamera2(InputAction.CallbackContext ctx){
        TrocarCamera2();
    }

    #endregion

}
