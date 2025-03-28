using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour{

    // Informações da câmera : 
    public ModoDeCamera modo;
    public enum ModoDeCamera {SINGLEPLAYER, MULTIPLAYER_LOCAL, MULTIPLAYER_ONLINE, INATIVO};
    [SerializeField] private CinemachineCamera[] cameras = new CinemachineCamera[2];
    InputActionMap inputActionMap;

    const float distance = 10, height = 40, FOV = 75;

    private void Awake(){
        ConfigurarCameras();
        DeterminaModoDeCamera();
    }

    private void Start(){
        inputActionMap =  GameManager.instance.input.Player.Get();
        SetMudançaDeCam();
    }

    private void SetMudançaDeCam(){
        inputActionMap["Move"].performed += TrocarCamera1;

        inputActionMap =  GameManager.instance.input.Player2.Get();
        inputActionMap["Move"].performed += TrocarCamera2;
    }

    void OnDisable(){
        inputActionMap["Move"].performed -= TrocarCamera1;
        inputActionMap["Move"].performed -= TrocarCamera2;
    }

    #region Configuração inicial

    private void ConfigurarCameras(){
        List<Player> players = GameManager.instance.jogadores;

        for (int i = 0; i < players.Count; i++){
            cameras[i].Follow = players[i].transform;
        }
    }

    // Alterna entre cameras.
    public void TrocarCamera1(){
        cameras[0].Priority = 1;
        cameras[1].Priority = 0;
    }

    void TrocarCamera1(InputAction.CallbackContext ctx){
        TrocarCamera1();
    }

    void TrocarCamera2(InputAction.CallbackContext ctx){
        TrocarCamera2();
    }
    
    public void TrocarCamera2(){
        cameras[1].Priority = 1;
        cameras[0].Priority = 0;
    }



    // Baseado no numero de jogadores determina qual o comportamento da câmera e o seu objeto alvo.
    private void DeterminaModoDeCamera(){

        switch(modo){

            case ModoDeCamera.INATIVO:                
            break;

            case ModoDeCamera.SINGLEPLAYER:
                Debug.Log("Câmera no modo Singleplayer");
                cameras[0].Priority = 1;
            break;

            case ModoDeCamera.MULTIPLAYER_LOCAL:
                Debug.Log("Câmera no modo Multiplayer");
            break;

            default:
                modo = ModoDeCamera.SINGLEPLAYER;
            break;

        }

    }

    #endregion

}
