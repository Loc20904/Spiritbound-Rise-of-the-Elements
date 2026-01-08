using UnityEngine;
public class PlayerMove : MonoBehaviour
{
    @Player_Move controls;
    Vector2 moveInput;

    void Awake()
    {
        controls = new @Player_Move();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => Jump();
    }

    void OnEnable() => controls.Player.Enable();
    void OnDisable() => controls.Player.Disable();

    void Update()
    {
        Debug.Log("Move = " + moveInput);
    }

    void Jump()
    {
        Debug.Log("Jump");
    }
}
