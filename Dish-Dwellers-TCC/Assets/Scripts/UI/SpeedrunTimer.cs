using UnityEngine;
using UnityEngine.UI;

public class SpeedrunTimer : MonoBehaviour {
    public Text speedrunText;

    void Start() {
        PauseUI pauseUI = GameManager.instance.uiManager.pauseUI;
        pauseUI.OnTimerHabilitadoChange += UpdateVisibilidade;
        UpdateVisibilidade(pauseUI.GetTimerHabilitado());
    }

    void OnDestroy() {
        PauseUI pauseUI = GameManager.instance?.uiManager?.pauseUI;
        if (pauseUI != null) pauseUI.OnTimerHabilitadoChange -= UpdateVisibilidade;
    }

    void UpdateVisibilidade(bool habilitado) {
        gameObject.SetActive(habilitado);
    }

    void LateUpdate() {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(GameManager.instance.gameTimer);
        speedrunText.text = string.Format("{0:D2}:{1:D2}.{2:D2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds/10);
    }
}
