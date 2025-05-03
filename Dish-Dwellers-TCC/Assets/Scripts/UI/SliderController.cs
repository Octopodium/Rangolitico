using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    public Slider sliderObject;

    public void Awake(){
        sliderObject.onValueChanged.AddListener(MudarValueSlider);
    }

    public void MudarValueSlider(float value){
        sliderObject.value = value;
    }

}
