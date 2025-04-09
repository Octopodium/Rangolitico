using UnityEngine;

public class Ponte : MonoBehaviour
{
    public Transform target; 
    public float speed = 2.0f; 

    private Vector3 initialPosition;
    private Vector3 destination;
    private bool isMoving = false;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                isMoving = false;
            }
        }
    }

    public void MoveToTarget()
    {
        if (target != null)
        {
            destination = target.position;
            isMoving = true;
        }
    }

    public void ReturnToStart()
    {
        destination = initialPosition;
        isMoving = true;
    }
}