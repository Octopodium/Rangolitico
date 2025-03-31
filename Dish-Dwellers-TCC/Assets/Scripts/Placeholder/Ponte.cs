using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public Transform target;
    public float speed = 2.0f; 
    private bool isMoving = false;

    void Update()
    {
        if (isMoving && target != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, target.position) < 0.01f)
            {
                isMoving = false;
            }
        }
    }

    public void StartMoving()
    {
        isMoving = true;
        Debug.Log("descendo");
    }
}