using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour{

    // Informações da câmera : 
    public ModoDeCamera modo;
    public enum ModoDeCamera {SINGLEPLAYER, MULTIPLAYER_LOCAL, MULTIPLAYER_ONLINE, INATIVO};
    [SerializeField] private Player[] players = new Player[2];
    [SerializeField] private CinemachineCamera[] cameras = new CinemachineCamera[2];
    [SerializeField] private GameObject cinemachineVCPrefab;

    const float distance = 10, height = 40, FOV = 75;

    private void Awake(){
        ConfigurarCameras();
        DeterminaModoDeCamera();
    }

    private void Start(){
        SetMudançaDeCam();
    }

    private void SetMudançaDeCam(){
        InputActionMap inputActionMap =  GameManager.instance.input.Player.Get();
        inputActionMap["Move"].performed += ctx => TrocarCamera(0);

        inputActionMap =  GameManager.instance.input.Player2.Get();
        inputActionMap["Move"].performed += ctx => TrocarCamera(1);
    }

    private void OnValidate(){
        ConfigurarCameras();
    }

    #region Configuração inicial

    private void ConfigurarCameras(){
        for (int i = 0; i < players.Length; i++){

            if(cameras[i].Follow == null){
                cameras[i].Follow = players[i].transform;
            }
            
        }
    }

    // Alterna entre cameras.
    public void TrocarCamera(int camera){
        if(cameras[camera].Priority == 1) return;

        Debug.Log("Mudando para camera " + camera);

        for( int i = 0; i < cameras.Length; i++){

            if(i == camera){
                cameras[i].Priority = 1;
                continue;
            }

            cameras[i].Priority = 0;
        }
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
