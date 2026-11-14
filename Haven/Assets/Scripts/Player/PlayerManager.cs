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
    public Collider2D attackHitbox;

    public static PlayerMovement Movement { get; private set; }
    public static PlayerCombat Combat { get; private set; }

    private void Awake()
    {
        // Init
        Instance = this;

        Inputs = new PlayerInputs();
        Inputs.Enable();
        playerObj = GameObject.FindGameObjectWithTag("Player");

        Movement = new(this);
        Combat = new(this);
    }
    private void Update()
    {
        Movement.Tick();
        Combat.Tick();
    }

    public bool TryStartAttack()
    {
        return Combat != null && Combat.TryStartAttack();
    }

    // Add these methods as animation events on the Player object's attack animation.
    public void AttackActiveStart()
    {
        Combat?.BeginActiveFrames();
    }

    public void AttackActiveEnd()
    {
        Combat?.BeginWinddown();
    }

    public void AttackWinddownEnd()
    {
        Combat?.BeginCooldown();
    }
}
