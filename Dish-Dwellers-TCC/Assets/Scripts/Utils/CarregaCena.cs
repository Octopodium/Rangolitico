using UnityEngine;
using UnityEngine.SceneManagement;

public class CarregaCena : MonoBehaviour
{
    public void CarregarCena(string nomeDaCena){
        SceneManager.LoadScene(nomeDaCena);
    }
}
