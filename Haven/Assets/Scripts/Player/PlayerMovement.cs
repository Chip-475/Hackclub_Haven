using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

[Serializable]
public class PlayerMovement
{
    GameObject playerObj;

    [SerializeField] float speed;
    [SerializeField] float jumpForce;

    [SerializeField] float dashSpeed;
    [SerializeField] bool canDash = true;
    [SerializeField] float dashDuration = 0.3f;
    [SerializeField] float dashTimer = 0f;

    Rigidbody2D rb;
    SpriteRenderer sr;

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
        sr = manager.spriteRenderer;
        groundChecker = manager.groundChecker;
        groundLayer = manager.groundLayer;
        var collider = playerObj.GetComponent<BoxCollider2D>();
        if (collider != null && groundChecker != null)
            groundChecker.localPosition = new Vector3(collider.offset.x, collider.offset.y - collider.size.y / 2f + .02f, 0);

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
        else Move();

        IsMoving = !Mathf.Approximately(rb.linearVelocityX, 0f);
        IsJumping = !IsGrounded();
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

        if (rb.linearVelocityX == 0)
            return;
        rb.transform.localScale = rb.linearVelocityX > 0 ? new Vector3(1, 1, 1) : new Vector3(-1, 1, 1);
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

    public void ResetActions()
    {
        IsDashing=false; IsMoving=false; IsJumping=false; canDash=true; dashTimer=0;
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
