using UnityEngine;

public class GameTime : MonoBehaviour {
    public float timer {get; private set;} = 0;
    public bool tocando = false;

    public System.Action OnPause, OnPlay, OnRestart;

    public void Restart() {
        timer = 0;
        OnRestart?.Invoke();
    }

    public void Play() {
        tocando = true;
        OnPlay?.Invoke();
    }

    public void Pause() {
        tocando = false;
        OnPause?.Invoke();
    }

    void Update() {
        if (!tocando) return;
        timer += Time.unscaledDeltaTime;
    }
}
