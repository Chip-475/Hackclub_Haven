using UnityEngine;

public class Swordsman : Enemy
{
    public void AI()
    {
        if (isDetectingPlayer)
        {
            MoveTo(PlayerManager.Instance.playerObj.transform.position);
        }
    }
}
