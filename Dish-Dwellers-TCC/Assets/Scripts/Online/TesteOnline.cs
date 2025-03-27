using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class TesteOnline : NetworkBehaviour {
    public Transform pai;
    public InputField inputField;
    public GameObject textPrefab;

    [Command]
    public void AdicionarTexto() {
        GameObject texto = Instantiate(textPrefab, pai);
        texto.GetComponent<Text>().text = inputField.text;
    }
}
