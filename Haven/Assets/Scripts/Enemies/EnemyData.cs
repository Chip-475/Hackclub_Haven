using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    public float maxHp;
    public float atk;
    public float def;
    public float spd;
    [Space]
    public float attackRange;
    public float detectionRange;
    public float preferredDistanceFromPlayer;
}
