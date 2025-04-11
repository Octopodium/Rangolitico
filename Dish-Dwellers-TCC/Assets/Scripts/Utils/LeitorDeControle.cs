using UnityEngine;
using UnityEngine.InputSystem;

public class LeitorDeControle : MonoBehaviour
{
    //Esquemas de UI por controle
    public GameObject gamepadUI;
    public GameObject keyboardUI;
    public GameObject prefabY, prefabE; //depois vou analisar a possibilidade de mudar pra 1 prefab e trocar so o sprite
    public Interagivel[] interagiveis;

    public Actions input;

    //pegar todos os interagiveis do jogo e mudar o prefab
    public void Start(){
        interagiveis = FindObjectsOfType(typeof(Interagivel)) as Interagivel[];
        input = GameManager.instance.input;
        input.Player.Get().actionTriggered += ChecaInput; //pega qualquer acao triggerada
    }

    public void ChecaInput(InputAction.CallbackContext ctx){
        InputDevice controle = ctx.control.device;

        if(controle is Gamepad){
            //para quando tiver indicacao de botoes na UI
            //keyboardUI.SetActive(false); gamepadUI.SetActive(true);

            if(prefabE.activeSelf) prefabE.SetActive(false); //para impedir conflitos de multiple devices

            foreach(Interagivel interagivel in interagiveis){
                interagivel.indicadorPrefab = prefabY;
            }
        }else if(controle is Keyboard){
            //gamepadUI.SetActive(false); keyboardUI.SetActive(true);
            
            if(prefabY.activeSelf) prefabY.SetActive(false);

            foreach(Interagivel interagivel in interagiveis){
                interagivel.indicadorPrefab = prefabE;
            }
        }
    }
}
