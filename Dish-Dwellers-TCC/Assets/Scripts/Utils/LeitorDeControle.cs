using UnityEngine;
using UnityEngine.InputSystem;

public class LeitorDeControle : MonoBehaviour
{
    //Esquemas de UI por controle
    public GameObject gamepadUI;
    public GameObject keyboardUI;
    public GameObject prefabY, prefabE;
    public Interagivel[] interagiveis;
    public string currentControlScheme;
    //public PlayerInput playerInput;

    //pegar todos os interagiveis do jogo e mudar o prefab

    public void Start(){
        interagiveis = FindObjectsOfType(typeof(Interagivel)) as Interagivel[];
        ChecaInput();
        //playerInput = GetComponent<PlayerInput>();

    }

    public void Udpate(){
        //currentControlScheme = playerInput.currentControlScheme;
        //Debug.Log("Esquema atual: " + playerInput.currentControlScheme);
        ChecaInput();
    }

    public void ChecaInput(){
        //nao funciona meu deus ja tentei todo tipo de leitor aaaaa como pode um ANYKEY nao LER ANYKEY
        bool isGamepad = Gamepad.current != null && (Gamepad.current.leftStick.ReadValue().magnitude > 0.1f || Gamepad.current.buttonSouth.isPressed);
        bool isKeyboard = Keyboard.current != null && (Keyboard.current.anyKey.isPressed || Mouse.current.delta.ReadValue().magnitude > 0);

        if(isGamepad){
            Debug.Log("gpad");
            //keyboardUI.SetActive(false);
            //gamepadUI.SetActive(true);
            if(prefabE.activeSelf) prefabE.SetActive(false);

            foreach(Interagivel interagivel in interagiveis){
                interagivel.indicadorPrefab = prefabY;
            }
        }else if(isKeyboard){
            Debug.Log("ley");
            //gamepadUI.SetActive(false);
            //keyboardUI.SetActive(true);
            if(prefabY.activeSelf) prefabY.SetActive(false);

            foreach(Interagivel interagivel in interagiveis){
                interagivel.indicadorPrefab = prefabE;
            }
        }
    }
}
