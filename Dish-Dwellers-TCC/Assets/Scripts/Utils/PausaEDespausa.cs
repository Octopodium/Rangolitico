using UnityEngine;

public class PausaEDespausa : MonoBehaviour
{
    public void PausaJogo(){
        Time.timeScale = 0;
    }
    public void DespausaJogo(){
        Time.timeScale = 1;
    }
}
