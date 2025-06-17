using System.Collections.Generic;
using UnityEngine;

public class SplitFollowTarget : MonoBehaviour {
    [SerializeField] private List<Transform> targetGroup = new List<Transform>(2);

    [Tooltip(@" 
                Pesos de valor 0 representam o comportamento 'padrão' naquele eixo, onde não existe uma preferência por movimento em nenhum sentido,
                valores maiores do que 0 indicam que a camera prefere estar no sentido positiva daquele eixo,
                valores menores do que 0 indicam que a camera prefere estar no sentido negativo daquele eixo.
            ")]
    [SerializeField] private float pesoX = 0.0f, pesoY = 0.25f, pesoZ = -0.25f;


    private void Start() {
        // esse é o jeito otimizado de atribuir um grupo, nesse caso especifico (eu acho né).
        foreach (var jogador in GameManager.instance.jogadores) {
            targetGroup.Add(jogador.transform);
        }
    }

    private void LateUpdate() {
        transform.position = CalcularPosMedia();
    }

    /// <summary>
    /// Retorna um Vetor que representa a distancia entre os alvos 
    /// </summary>
    /// <returns></returns>
    public Vector3 CalcularDistancia() {
        return targetGroup[0].position - targetGroup[1].position;
    }

    private Vector3 CalcularPosMedia() {
        Vector3 fPos;
        Vector3 dist = CalcularDistancia();

        //Encontra a posição media entre os dois transforms:
        fPos.x = (targetGroup[0].position.x + targetGroup[1].position.x) * 0.5f;
        fPos.y = (targetGroup[0].position.y + targetGroup[1].position.y) * 0.5f;
        fPos.z = (targetGroup[0].position.z + targetGroup[1].position.z) * 0.5f;

        // Aplica o peso à posição média encontrada, alterando a posição final da camera:
        if (pesoX != 0)
            fPos.x += Mathf.Abs(dist.x) * pesoX;
        if (pesoY != 0)
            fPos.y += Mathf.Abs(dist.y) * pesoY;
        if (pesoZ != 0)
            fPos.z += Mathf.Abs(dist.z) * pesoZ;
        return fPos;
    }
}
