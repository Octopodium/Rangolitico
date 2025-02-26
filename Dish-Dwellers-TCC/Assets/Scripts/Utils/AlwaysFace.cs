using UnityEngine;

// Faz algo sempre encarar a camera ou outro objeto (muito bom para indicadores)
public class AlwaysFace : MonoBehaviour {
    public bool lockX = false, lockY = false, lockZ = false;
    public bool faceCamera = true;
    public GameObject targetObj;

    void FixedUpdate() {
        Vector3 target;
        
        if (targetObj) target = targetObj.transform.position;
        else if (faceCamera && Camera.main != null) target = Camera.main.transform.position;
        else return;

        Vector3 direction = target - transform.position;
        if (lockX) direction.x = 0;
        if (lockY) direction.y = 0;
        if (lockZ) direction.z = 0;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
