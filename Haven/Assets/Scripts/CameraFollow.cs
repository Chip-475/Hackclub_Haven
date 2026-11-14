using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Camera main;

    private void Awake() => main = Camera.main;
    private void LateUpdate()
    {
        if (main == null)
            return;

        Vector3 playerPos = PlayerManager.Instance.playerObj.transform.position;
        main.transform.position = new Vector3(
            playerPos.x,
            playerPos.y,
            main.transform.position.z
        );
    }
}
