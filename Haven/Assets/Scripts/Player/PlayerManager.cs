using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public static PlayerInputs Inputs;
    public GameObject playerObj;
    public PlayerData playerData;
    [Space]
    public Transform groundChecker;
    public LayerMask groundLayer;

    public SpriteRenderer spriteRenderer;
    public Animator animator;
    public Collider2D hitCollider;

    public PlayerMovement Movement;
    public PlayerCombat Combat;
    public PlayerAnimations Animations;

    public PlayerAnimations.AnimationState state = PlayerAnimations.AnimationState.Idle;

    private void Awake()
    {
        // Init
        Instance = this;

        Inputs = new PlayerInputs();
        Inputs.Enable();
        playerObj = GameObject.FindGameObjectWithTag("Player");

        Movement = new(this);
        Combat = new(this);
        Animations = new(this);

        Inputs.Combat.Attack.performed += ctx => StartCoroutine(Combat.PlayerAttack());
    }
    private void Update()
    {
        Movement.Tick();
        Combat.Tick();
    }
}
