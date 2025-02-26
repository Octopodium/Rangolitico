using UnityEngine;

public class Movimentacao : MonoBehaviour {
    public CharacterController controller;
    public float speed = 12f;

    void Update() {
        Vector2 moveInput = GameManager.instance.input.Player.Move.ReadValue<Vector2>();
        float x = moveInput.x;
        float z = moveInput.y;

        Vector3 move = transform.right * x + transform.forward * z;

        if (!controller.isGrounded) move.y = -9f;

        controller.Move(move * speed * Time.deltaTime);
    }
}
