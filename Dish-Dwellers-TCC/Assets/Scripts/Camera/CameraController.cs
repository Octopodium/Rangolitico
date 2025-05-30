using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
public class CameraController : MonoBehaviour{

    // Informações da câmera : 
    ModoDeJogo modoDeJogoConfigurado = ModoDeJogo.INDEFINIDO;
    public bool ativo = true; // Substitui o modo "INATIVO" previamente implementado.
    [SerializeField] private CinemachineCamera[] ccameras = new CinemachineCamera[2];
    [SerializeField] private CinemachineCamera introCamera;
    [SerializeField] private Camera[] cameras = new Camera[2];
    private float tempoDBlendNormal = 0.5f, tempoDBlendIntro = 2.0f;
    public UnityEvent onTerminarIntro;
    private bool podeTrocarCamera = false;
    private bool splitScreen = false;


    private void Start(){
        DeterminaModoDeCamera();
        if(introCamera != null) 
            FazerIntroducao();
        else {
            podeTrocarCamera = true;
            ConfigurarCameras();
        }
    }

    private void DeterminaModoDeCamera(){
        if (!ativo) return;

        modoDeJogoConfigurado = GameManager.instance.modoDeJogo;

        switch (modoDeJogoConfigurado) {
            case ModoDeJogo.SINGLEPLAYER:
                if (introCamera) {
                    onTerminarIntro.AddListener(() => {
                        GameManager.instance.OnTrocarControle += TrocarCamera;
                        TrocarCamera(GameManager.instance.playerAtual);
                    });
                } else {
                    GameManager.instance.OnTrocarControle += TrocarCamera;
                }
                break;

            case ModoDeJogo.MULTIPLAYER_LOCAL:
                UsarSegundaCam();
                if (!introCamera)
                    cameras[0].rect = new Rect(-0.5f, 0.0f, 1, 1);
                break;

            case ModoDeJogo.MULTIPLAYER_ONLINE:
                TrocarCamera1();
                break;
        }

    }

    private void UsarSegundaCam(){
        cameras[1].gameObject.SetActive(true);
        ccameras[1].OutputChannel = OutputChannels.Channel02;

        foreach(CinemachineCamera cam in ccameras){
            cam.Priority = 1;
        }

        splitScreen = true;
    }

    void OnDisable(){
        switch(modoDeJogoConfigurado) {
            case ModoDeJogo.SINGLEPLAYER:
                GameManager.instance.OnTrocarControle -= TrocarCamera;    
            break;

            case ModoDeJogo.MULTIPLAYER_LOCAL:
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
        SetTempoDeBlend(tempoDBlendIntro);

        introCamera.Priority = 2;

        yield return new WaitForSeconds(1.0f);

        introCamera.Priority = 0;
        ccameras[0].Priority = 1;
        if(splitScreen) ccameras[1].Priority = 1;

        if(splitScreen){
            float timer = 2f; // Timer utilizado pra fazer a transição de camera full screen para split screen.
            float interpolador; // coeficiente de interpolação
            float y; // Valor que vai ser aplicado ao componente y do rect da camera.

            while(timer > 0){
                timer -= Time.deltaTime;
                interpolador = timer / 2;
                y = Mathf.Lerp(-0.5f, 0.05f, interpolador);

                Debug.Log(y);

                cameras[0].rect = new Rect(y, 0, 1, 1);
                cameras[1].rect = new Rect(1 + y, 0, 1, 1);

                yield return new WaitForFixedUpdate();
            }
            Debug.Log("Terminou");
            cameras[0].rect = new Rect( -0.5f, 0, 1, 1);
            cameras[1].rect = new Rect( 0.5f, 0, 1, 1);
        }

        else{
            yield return new WaitForSeconds(2.1f);
        }

        
        SetTempoDeBlend(tempoDBlendNormal);

        podeTrocarCamera = true;
        onTerminarIntro?.Invoke();
        ConfigurarCameras();
    }


    /// <summary>
    /// Muda o tempo de blend da camera(s) a depender do modo de camera.
    /// </summary>
    /// <param name="duracao"></param>
    private void SetTempoDeBlend(float duracao){
        CinemachineBrain brain;
        if(splitScreen){
            foreach(Camera cam in cameras){
                brain = cam.GetComponent<CinemachineBrain>();
                brain.DefaultBlend.Time = duracao;
            }
        }
        else{
            brain = cameras[0].GetComponent<CinemachineBrain>();
            brain.DefaultBlend.Time = duracao;
        }
    }

    #endregion

    #region Configuração inicial

    private void ConfigurarCameras(){
        List<Player> players = GameManager.instance.jogadores;

        if (modoDeJogoConfigurado == ModoDeJogo.MULTIPLAYER_ONLINE) {
            Player jogador = players[0].isLocalPlayer ? players[0] : players[1];
            Player outro_jogador = players[0].isLocalPlayer ? players[1] : players[0];

            ccameras[0].Follow = jogador.transform;
            ccameras[1].Follow = outro_jogador.transform;

            return;
        }

        for (int i = 0; i < players.Count; i++){
            if (players[i].qualPlayer == QualPlayer.Player1) ccameras[0].Follow = players[i].transform;
            else ccameras[1].Follow = players[i].transform;
        }
    }

    // Alterna entre cameras.
    public void TrocarCamera(QualPlayer player){
        if (player == QualPlayer.Player1) TrocarCamera1();
        else TrocarCamera2();
    }

    public void TrocarCamera1(){
        if(!podeTrocarCamera) return;
        ccameras[0].Priority = 1;
        ccameras[1].Priority = 0;
    }

    void TrocarCamera1(InputAction.CallbackContext ctx){
        TrocarCamera1();
    }

    public void TrocarCamera2(){
        if(!podeTrocarCamera) return;
        ccameras[1].Priority = 1;
        ccameras[0].Priority = 0;
    }

    void TrocarCamera2(InputAction.CallbackContext ctx){
        TrocarCamera2();
    }

    #endregion

}
