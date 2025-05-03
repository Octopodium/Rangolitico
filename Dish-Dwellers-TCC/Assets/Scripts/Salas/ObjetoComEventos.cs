using UnityEngine;
using UnityEngine.Events;

public class ObjetoComEventos : MonoBehaviour
{
    public UnityEvent onDestroy;
    public UnityEvent onDisable;
    public UnityEvent onEnable;
    public UnityEvent onStart;


    private void OnEnable() => onEnable?.Invoke();
    private void Start() => onStart?.Invoke();
    private void OnDisable() => onDisable?.Invoke();
    private void OnDestroy() => onDestroy?.Invoke();

}
