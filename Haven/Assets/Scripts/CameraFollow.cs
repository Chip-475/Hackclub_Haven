using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    Camera main;
    Vector3 velocity;
    Vector3 position;
    void Awake() { main=Camera.main; if(main!=null) position=main.transform.position; }
    void LateUpdate()
    {
        if(main==null || PlayerManager.Instance==null) return;
        var director=GameplayDirector.Instance;
        if(director!=null && director.IsBouncing) { position=main.transform.position; velocity=Vector3.zero; return; }
        Vector3 target=PlayerManager.Instance.playerObj.transform.position;
        float blend=Mathf.SmoothStep(0,1,Mathf.InverseLerp(28,35,target.x));
        target.x=Mathf.Lerp(target.x,41,blend);
        target.y=Mathf.Lerp(target.y,-.5f,blend); target.z=main.transform.position.z;
        position=Vector3.SmoothDamp(position,target,ref velocity,.25f);
        main.orthographicSize=Mathf.Lerp(main.orthographicSize,Mathf.Lerp(5,6.3f,blend),1-Mathf.Exp(-3*Time.deltaTime));
        Vector2 shake=director!=null?Random.insideUnitCircle*director.Shake:Vector2.zero;
        main.transform.position=position+(Vector3)shake;
    }
}
