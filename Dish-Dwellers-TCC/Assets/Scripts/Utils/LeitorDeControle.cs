using UnityEngine;
using UnityEngine.InputSystem;

public class LeitorDeControle : MonoBehaviour
{
    //Esquemas de UI por controle
    public GameObject gamepadUI;
    public GameObject keyboardUI;
    public GameObject prefabY, prefabE; //depois vou analisar a possibilidade de mudar pra 1 prefab e trocar so o sprite

    public System.Action<InputDevice> OnDeviceChanged; //para quando o jogador mudar de controle
    public System.Action<GameObject> OnIndicadorChange;

    public Actions input;
    public InputDevice controleAtual { get; private set; }
    public GameObject indicadorAtual { get { return controleAtual is Gamepad ? prefabY : prefabE; } }

    //pegar todos os interagiveis do jogo e mudar o prefab
    public void Start(){
        input = GameManager.instance.input;
        input.Player.Get().actionTriggered += ChecaInput; //pega qualquer acao triggerada
    }

    public void ChecaInput(InputAction.CallbackContext ctx){
        controleAtual = ctx.control.device;
        OnDeviceChanged?.Invoke(controleAtual);

        OnIndicadorChange?.Invoke(indicadorAtual);
    }
}
