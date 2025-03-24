using UnityEngine;

public class CameraMultiplayer : IComportamentoCamera{

    public Transform GetAlvo(Transform[] alvos){
        var bound = new Bounds(alvos[0].transform.position, Vector3.zero);

        foreach(var alvo in alvos){
            bound.Encapsulate(alvo.position);
        }

        var ponto = new GameObject("AlvoCamera");
        ponto.transform.position = bound.center;
        return ponto.transform;
    }

}
