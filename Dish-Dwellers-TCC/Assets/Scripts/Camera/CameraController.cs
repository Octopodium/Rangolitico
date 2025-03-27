using Unity.Cinemachine;
using UnityEngine;
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
    }

    #region Configuração inicial

    private void ConfigurarCameras(){

        for (int i = 0; i < players.Length; i++){
            cameras[i] = Instantiate(cinemachineVCPrefab).GetComponent<CinemachineCamera>();
            cameras[i].Follow = players[i].transform;
        }

    }

    public void TrocarCamera(){
        
    }


    // Baseado no numero de jogadores determina qual o comportamento da câmera e o seu objeto alvo.
    private void DeterminaModoDeCamera(){

        switch(modo){

            case ModoDeCamera.INATIVO:                
            break;

            case ModoDeCamera.SINGLEPLAYER:
                Debug.Log("Câmera no modo Singleplayer");
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
