using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BotaoSelecionavel : MonoBehaviour, IPointerEnterHandler {
    Button botao;

    void Awake() {
        botao = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (botao.interactable) {
            botao.Select();
        }
    }
}
