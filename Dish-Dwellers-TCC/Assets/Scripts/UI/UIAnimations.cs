using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIAnimations : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler{
    Button buttonText;
    public Image underline;
    public Vector3 buttonScaleFactor = new Vector3(1.1f, 1.1f, 1.1f); //o quanto queremos escalar
    public Vector3 buttonDefaultFactor = new Vector3(1f, 1f, 1f); //escala default
    Vector3 buttonInitialFactor = new Vector3(1f, 1f, 1f); //guarda o ultimo valor da escala
    public GameObject objectToScale;
    public Vector3 objectInitialFactor;
    public float objectScaleDuration = 0.2f;

    void Awake(){
        buttonText = GetComponent<Button>();
        buttonInitialFactor = buttonDefaultFactor;
    }

    public void OnSelect(BaseEventData eventData){
        HandleAnimation(0.1f, buttonScaleFactor);
        HandleFillUnderline(true);
    }

    public void OnDeselect(BaseEventData eventData){
        HandleAnimation(0.1f, buttonDefaultFactor);
        HandleFillUnderline(false);
    }

    public void OnPointerEnter(PointerEventData eventData){
        HandleAnimation(0.1f, buttonScaleFactor);
        HandleFillUnderline(true);
    }

    public void OnPointerExit(PointerEventData eventData){
        HandleAnimation(0.1f, buttonDefaultFactor);
        HandleFillUnderline(false);
    }
     
    IEnumerator ScaleText(float duration, Vector3 endScale){
        float tempo = 0f;
        while (tempo < duration){
            float animDurantion = tempo / duration;
            buttonText.gameObject.transform.localScale = Vector3.Lerp(buttonInitialFactor, endScale, animDurantion);
            tempo += Time.unscaledDeltaTime;
            yield return null;
        }
        buttonText.gameObject.transform.localScale = endScale;
        buttonInitialFactor = endScale;
    }

    IEnumerator FillUnderline(float amount, bool fill){
        if(fill){
            while(underline.fillAmount != 1.0f){
                underline.fillAmount += amount;
                yield return null;
            }
        }else{
            while(underline.fillAmount != 0.0f){
                underline.fillAmount -= amount;
                yield return null;
            }
        }
    }

    public void HandleAnimation(float duration, Vector3 endScale){
        if (buttonText != null){
            StopAllCoroutines();
            StartCoroutine(ScaleText(duration, endScale));
        }
    }

    public void HandleFillUnderline(bool fill){
        if (underline != null){
            StartCoroutine(FillUnderline(0.04f, fill));
        }
    }

    /*public void HandleScaleIn(){
        objectInitialFactor = objectToScale.transform.localScale;
        //objectToScale.transform.localScale = Vector3.zero;
        StartCoroutine("ScaleIn");
    }*/

    public void HandleScaleOut(){
        
    }

    IEnumerator ScaleIn(){
        float tempo = 0f;
        while (tempo < objectScaleDuration){
            float animDurantion = tempo / objectScaleDuration;
            objectToScale.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, animDurantion);
            tempo += Time.unscaledDeltaTime;
            yield return null;
        }
        objectToScale.transform.localScale = Vector3.one;
    }
}
