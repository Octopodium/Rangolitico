using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AnimationAgent : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;

    [Header("Game Information"), Space(10)]
    public static readonly int WalkSpeed = Animator.StringToHash(nameof(WalkSpeed));
    public static readonly int Falling = Animator.StringToHash(nameof(Falling));
    public static readonly int Carrying = Animator.StringToHash(nameof(Carrying));
    public static readonly int Throw = Animator.StringToHash(nameof(Throw));
    public static readonly int Dead = Animator.StringToHash(nameof(Dead));
    public static readonly int Damage = Animator.StringToHash(nameof(Damage));
    public bool iddle, run , change;
    
    

    private void Start(){
        animator = GetComponent<Animator>();
    }

    [Tooltip("Set moving animation in accordance with the speed value informed.")]
    public void Move(Vector3 speed, float maxSpeed){

        if(speed.x > 0){ // Turn character to the right
            transform.localScale = Vector3.Scale(transform.localScale, Vector3.right);
        }
        else if(speed.x < 0){// Turn character to the left
            transform.localScale = Vector3.Scale(transform.localScale, Vector3.left);
        }
        if(speed.z > 0){// Turn character to the front            
        }
        else if(speed.z < 0){ // Turn character to the back            
        }

        float relativeSpeed = ((speed.x * speed.x) + (speed.z * speed.z)) / (maxSpeed * maxSpeed);
        animator.SetFloat(WalkSpeed, relativeSpeed);
    }

    private void Update(){
        if(change){
            float sinVal = Mathf.PingPong(Time.time, 1);
            animator.SetFloat(WalkSpeed, sinVal);
        }
        else if(run){
            animator.SetFloat(WalkSpeed, 1);
        }
        else if(iddle){
        }
    }
}
