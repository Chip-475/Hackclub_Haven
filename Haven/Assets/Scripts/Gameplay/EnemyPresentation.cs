using UnityEngine;

[DisallowMultipleComponent]
public class EnemyPresentation : MonoBehaviour
{
    IDamageable target;
    Collider2D body;
    Enemy enemy;
    BossController boss;
    void Start()
    {
        target=GetComponent<IDamageable>();
        enemy=GetComponent<Enemy>(); boss=GetComponent<BossController>();
        body=GetComponent<Collider2D>();
        // Only ignore solid body pairs. Trigger hitboxes and damage queries remain active.
        var player=PlayerManager.Instance;
        if(player==null) return;
        foreach(var own in GetComponentsInChildren<Collider2D>())
            foreach(var other in player.playerObj.GetComponentsInChildren<Collider2D>())
                if(!own.isTrigger && !other.isTrigger) Physics2D.IgnoreCollision(own,other,true);
    }
    void OnGUI()
    {
        var camera=Camera.main; var player=PlayerManager.Instance;
        if(target==null || target.Health<=0 || body==null || camera==null || player==null) return;
        if(GameplayDirector.Instance!=null && GameplayDirector.Instance.IsBouncing) return;
        float visibility=1;
        if(player.playerObj.transform.position.x<32)
            visibility=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(4.5f,9.5f,Vector2.Distance(transform.position,player.playerObj.transform.position)));
        Vector3 point=camera.WorldToScreenPoint(new Vector3(body.bounds.center.x,body.bounds.max.y+.3f,0));
        if(point.z<0 || point.x<0 || point.x>Screen.width || point.y<0 || point.y>Screen.height || visibility<=0) return;
        float maximum=boss!=null?boss.MaxHealth:enemy!=null?enemy.MaxHealth:target.Health;
        float width=boss!=null?140:64;
        Rect rect=new Rect(point.x-width/2,Screen.height-point.y-8,width,8);
        GUI.depth=-60; Color old=GUI.color;
        GUI.color=new Color(.06f,.04f,.04f,visibility); GUI.DrawTexture(rect,Texture2D.whiteTexture);
        GUI.color=new Color(.8f,.16f,.12f,visibility);
        GUI.DrawTexture(new Rect(rect.x+1,rect.y+1,(width-2)*Mathf.Clamp01(target.Health/Mathf.Max(1,maximum)),6),Texture2D.whiteTexture);
        GUI.color=old;
    }
}
