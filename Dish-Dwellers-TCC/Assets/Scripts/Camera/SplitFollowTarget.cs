using System.Collections.Generic;
using UnityEngine;

public class SplitFollowTarget : MonoBehaviour {
    [SerializeField] private List<Transform> targetGroup = new List<Transform>(2);


    private void Start() {
        // esse é o jeito otimizado de atribuir um grupo, nesse caso especifico (eu acho né).
        foreach (var jogador in GameManager.instance.jogadores) {
            targetGroup.Add(jogador.transform);
        }
    }

    private void LateUpdate() {
        transform.position = CalcularPosMedia();
    }

    public Vector3 CalcularDistancia() {
        return targetGroup[0].position - targetGroup[1].position;
    }

    private Vector3 CalcularPosMedia() {
        Vector3 fPos;
        Vector3 dist = CalcularDistancia();

        fPos.x = (targetGroup[0].position.x + targetGroup[1].position.x) * 0.5f;
        fPos.y = (targetGroup[0].position.y + targetGroup[1].position.y) * 0.5f + (Mathf.Abs(dist.y) * 0.25f) ;
        fPos.z = (targetGroup[0].position.z + targetGroup[1].position.z) * 0.5f - (Mathf.Abs(dist.z) * 0.25f) ;

        return fPos;
    }
}
