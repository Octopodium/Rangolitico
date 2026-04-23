using UnityEngine;
using UnityEngine.UI;

public class TelaDeFim : MonoBehaviour {
    public GameObject timerHolder;
    public Text timer;

    void Start() {
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(GameManager.ultimoTimer);
        timer.text = string.Format("{0:D2}:{1:D2}.{2:D2}", timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds/10);
    }
    
}
