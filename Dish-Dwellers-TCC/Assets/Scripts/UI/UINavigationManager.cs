using UnityEngine;
using UnityEngine.EventSystems;

public class UINavigationManager : MonoBehaviour
{
    public EventSystem eventSystem;
    public GameObject currentFirstSelected;

    public void TrocaFirstSelected(GameObject firstSelected){
        eventSystem.SetSelectedGameObject(firstSelected);
    }
}
