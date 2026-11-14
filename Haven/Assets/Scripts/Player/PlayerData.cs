using System.Runtime.InteropServices;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Data", menuName = "Player Data")]
public class PlayerData : ScriptableObject
{
    public float maxHp;
    public float atk;
    public float def;
    public float spd;
    public float jumpForce;
    public float dashSpeed;
}
