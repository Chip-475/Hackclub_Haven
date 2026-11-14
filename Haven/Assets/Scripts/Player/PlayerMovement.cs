using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement
{
    GameObject playerObj;

    float speed;
    float jumpForce;

    float dashSpeed;
    bool canDash = true;
    float dashDuration = 0.3f;
    float dashTimer = 0f;

    Rigidbody2D rb;

    Transform groundChecker;
    LayerMask groundLayer;

    public bool IsMoving { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsDashing { get; private set; }

    public PlayerMovement(PlayerManager manager)
    {
        playerObj = manager.playerObj;
        speed = manager.playerData.spd;
        jumpForce = manager.playerData.jumpForce;
        dashSpeed = manager.playerData.dashSpeed;

        rb = playerObj.GetComponent<Rigidbody2D>();
        groundChecker = manager.groundChecker;
        groundLayer = manager.groundLayer;

        PlayerManager.Inputs.Movement.Jump.performed += ctx => Jump();
        PlayerManager.Inputs.Movement.Dash.performed += ctx => Dash();
    }

    public void Tick()
    {
        if (!canDash)
            canDash = IsGrounded();

        if (IsDashing)
        {
            dashTimer -= Time.deltaTime;
            IsDashing = dashTimer >= 0;
        }
        else
        {
            Move();
        }
    }

    void Move()
    {
        if (IsDashing)
            return;

        Vector2 moveInput = PlayerManager.Inputs.Movement.Move.ReadValue<Vector2>();
        rb.linearVelocity = new Vector2(
            moveInput.x * speed,
            rb.linearVelocity.y
        );

        // Sprite flip logic
    }
    void Jump()
    {
        if (IsDashing)
            return;

        if (!IsGrounded())
            return;

        if (PlayerManager.Inputs.Movement.Jump.triggered)
            rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
    }
    void Dash()
    {
        if (IsDashing || !canDash)
            return;

        IsDashing = true;
        canDash = false;
        dashTimer = dashDuration;
        Vector2 moveInput = PlayerManager.Inputs.Movement.Move.ReadValue<Vector2>();
        rb.AddForce(moveInput * dashSpeed, ForceMode2D.Impulse);
    }

    bool IsGrounded()
    {
        Vector2 origin = groundChecker.position;
        float groundCheckDistance = 0.1f;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        Debug.DrawRay(origin, Vector2.down * groundCheckDistance,
                      hit.collider != null ? Color.green : Color.red);

        return hit.collider != null;
    }
}
