using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraController : MonoBehaviour{

    // Informações da câmera : 
    private enum ModoDeCamera {SINGLEPLAYER, MULTIPLAYER, INATIVO};
    private ModoDeCamera modo;
    private GameObject[] jogadores;
    private Transform alvo;
    const float distance = 16, height = 40, FOV = 75;

    // Componentes :
    private CinemachineCamera cinemachine;


    private void Awake(){
        cinemachine = GetComponent<CinemachineCamera>();

    }

    private void Start(){
        DeterminaModoDeCamera();
    }

    private void LateUpdate(){
        if(modo == ModoDeCamera.MULTIPLAYER){
            alvo.position = PosicaoMediaDosJogadores();       
        }
    }

    private Vector3 PosicaoMediaDosJogadores(){
        Vector3 medPos = new Vector3();

        foreach(var jogador in jogadores){
            medPos += jogador.transform.position;
        }

        medPos /= jogadores.Length;

        return medPos;
    }

    #region Configuração inicial

    // Baseado no numero de jogadores determina qual o comportamento da câmera e o seu objeto alvo.
    private void DeterminaModoDeCamera(){

        int nJogadores = NumeroDeJogadores();

        switch(nJogadores){

            case 0:
                modo = ModoDeCamera.INATIVO;
            throw new System.Exception("Nenhum Jogador foi encontrado pela câmera. Verifique a existencia dos jogadores na cena e se as tags estão corretas.");

            case 1:
                Debug.Log("Câmera no modo Singleplayer");
                modo = ModoDeCamera.SINGLEPLAYER;
                alvo = jogadores[0].transform;
            break;

            case 2:
                Debug.Log("Câmera no modo Multiplayer");
                modo = ModoDeCamera.MULTIPLAYER;
                alvo = new GameObject("Alvo_CameraMultiplayer").transform;
            break;

            default:
                Debug.Log("Câmera no modo Multiplayer, <color=red>mas tem mais do que dois players na cena</color>");
                modo = ModoDeCamera.MULTIPLAYER;
                alvo = new GameObject("Alvo_CameraMultiplayer").transform;
            break;

        }
        Debug.Log($"Modo de camera : {modo}");

        cinemachine.Follow = alvo;
    }

    private int NumeroDeJogadores(){
        jogadores = GameObject.FindGameObjectsWithTag("Player");
        return jogadores.Length;
    }

    #endregion

}
