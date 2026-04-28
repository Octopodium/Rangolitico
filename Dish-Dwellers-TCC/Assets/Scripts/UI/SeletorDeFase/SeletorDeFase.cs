using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class SeletorDeFase : MonoBehaviour {
    public bool irParaCena = true;

    public System.Action<SalaInfo> salaSelecionada;

    public UnityEvent OnFechado;
    public EventSystem eventSystem;
    public GameObject primeiraSelecao;


    void Awake() {
        if (eventSystem == null) {
            eventSystem = FindFirstObjectByType<EventSystem>();
        }
    }

    public void Selecionar(GameObject selecao) {
        eventSystem.SetSelectedGameObject(selecao);
    }

    public void Mostrar() {
        gameObject.SetActive(true);
        Selecionar(primeiraSelecao);
    }

    public void SalaSelecionada(SalaInfo sala) {
        salaSelecionada?.Invoke(sala);
        if (irParaCena) IrParaSala(sala.caminhoParaSala);
    }

    public void IrParaSala(string salaFase){
        if(salaFase != null){
            SceneManager.LoadScene(salaFase, LoadSceneMode.Single);
        }
    }

    public void HandleFechar() {
        OnFechado?.Invoke();
    }

}
