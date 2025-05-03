using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playeer : MonoBehaviour {
    Actions input;
    void Start() {
        input = new Actions();
        input.Player.Enable();
    }

    void Update() {
        Vector2 move = input.Player.Move.ReadValue<Vector2>();

        transform.position += new Vector3(move.x, 0, move.y) * Time.deltaTime;
    }
}
