using UnityEngine;

public class CameraSinglePlayer : IComportamentoCamera{

    public Transform GetAlvo(Transform[] alvos){
        return alvos[0];
    }

}
