using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraController : MonoBehaviour
{
    private GameObject[] jogadores;
    private GameObject alvo;
    private CinemachineCamera cinemachine;


    private void Awake(){
        cinemachine = GetComponent<CinemachineCamera>();
    }

    private void Start(){
        jogadores = GameObject.FindGameObjectsWithTag("Player");
        if(alvo == null){
            alvo = GameObject.FindWithTag("CameraAlvo");
        }
        alvo.transform.position = CalcularPosicaoMedia();
        //cinemachine.Target = alvo;
    }


    // Calcula a posição média entre todos os jogadores presentes na cena
    private Vector3 CalcularPosicaoMedia(){
        Vector3 posMed = new Vector3();

        foreach(var jogador in jogadores){
            posMed += jogador.transform.position; 
        }

        posMed /= jogadores.Length;
        return posMed;
    }
}
