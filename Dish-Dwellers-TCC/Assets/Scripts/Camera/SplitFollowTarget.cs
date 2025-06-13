using System.Collections.Generic;
using UnityEngine;

public class SplitFollowTarget : MonoBehaviour {
    [SerializeField] private List<Transform> targetGroup = new List<Transform>(2);
    [SerializeField] private float distance;


    private void Start() {
        // esse é o jeito otimizado de atribuir um grupo, nesse caso especifico (eu acho né).
        foreach (var jogador in GameManager.instance.jogadores) {
            targetGroup.Add(jogador.transform);
        }
    }

    private void LateUpdate() {
        transform.position = CalcularPosMedia();
        distance = CalcularDistancia();
    }

    public float CalcularDistancia() {
        return (targetGroup[0].position - targetGroup[1].position).magnitude;
    }

    private Vector3 CalcularPosMedia() {
        Vector3 fPos;

        fPos = targetGroup[0].position + targetGroup[1].position;
        fPos /= 2;


        return fPos;
    }
}
