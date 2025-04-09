using UnityEngine;

public class Ponte2 : MonoBehaviour
{
    public Animator animator; // arraste o Animator no Inspector

    public void PlayAnimation()
    {
        animator.SetTrigger("Play");
    }
}