using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static Pathfinder UniversalPathfinder { get; private set; }

    private void Awake()
    {
        Instance = this;
        UniversalPathfinder = new(500, 500);
    }
}
