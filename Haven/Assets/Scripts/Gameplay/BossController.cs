using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour, IDamageable
{
    public enum AnimationState { Spawn, Idle, Walking, Attack, Dead }
    public AnimationState state;
    public float Health { get; private set; } = 35f;
    public float Defense => 0;
    SpriteRenderer visual;
    Sprite[] spawn, walk, attack;
    float clock, cooldown;
    bool hit;
    int facing = -1;
    Sprite[] Frames(string name, int count)
    {
        var texture=Resources.Load<Texture2D>("Boss/"+name);
        if(texture==null) { Debug.LogError("Missing boss sheet: "+name); return new Sprite[0]; }
        var frames=new Sprite[count]; int width=texture.width/count;
        for(int i=0;i<count;i++) frames[i]=Sprite.Create(texture,new Rect(i*width,0,width,texture.height),new Vector2(.5f,0),texture.height/4.8f);
        return frames;
    }
    void Awake()
    {
        visual=gameObject.AddComponent<SpriteRenderer>(); visual.sortingOrder=5;
        spawn=Frames("Spawn",5); walk=Frames("Walk",4); attack=Frames("Attack",3);
        var collider=gameObject.AddComponent<BoxCollider2D>(); collider.size=new Vector2(1.2f,3.7f); collider.offset=new Vector2(0,1.85f);
        gameObject.tag="Enemy"; gameObject.layer=LayerMask.NameToLayer("Enemy");
    }
    IEnumerator Start()
    {
        state=AnimationState.Spawn;
        for(int i=0;i<spawn.Length;i++)
        {
            visual.sprite=spawn[i]; visual.flipX=true;
            if(i==1 || i==2) { GameplayDirector.Instance.Impact(); yield return new WaitForSeconds(.07f); }
            yield return new WaitForSeconds(i==0?.35f:.16f);
        }
        state=AnimationState.Idle;
    }
    void Update()
    {
        if(state==AnimationState.Spawn || state==AnimationState.Dead || Time.timeScale==0) return;
        var player=PlayerManager.Instance;
        if(player==null || player.IsRespawning) return;
        float dx=player.playerObj.transform.position.x-transform.position.x;
        if(state==AnimationState.Attack)
        {
            clock+=Time.deltaTime; Show(attack,Mathf.Min(2,(int)(clock/.25f)));
            if(!hit && clock>=.25f)
            {
                hit=true; AudioManager.Instance?.Impact();
                var pos=player.playerObj.transform.position;
                if(Mathf.Abs(dx)<3.1f && dx*facing>=0 && pos.y<transform.position.y+3.8f && pos.y>transform.position.y-.5f) player.TakeDamage(2);
            }
            if(clock>=.8f) { state=AnimationState.Idle; cooldown=Time.time+1.2f; }
            return;
        }
        facing=dx<0?-1:1; visual.flipX=facing<0;
        if(player.playerObj.transform.position.x<32) { state=AnimationState.Idle; Show(walk,0); return; }
        if(Mathf.Abs(dx)>2.5f)
        {
            state=AnimationState.Walking; clock+=Time.deltaTime;
            transform.position=new Vector3(Mathf.Clamp(transform.position.x+facing*1.7f*Time.deltaTime,34,48),transform.position.y,0);
            Show(walk,(int)(clock*6)%4);
        }
        else if(Time.time>=cooldown) { state=AnimationState.Attack; clock=0; hit=false; }
        else { state=AnimationState.Idle; Show(walk,0); }
    }
    void Show(Sprite[] frames,int index) { if(frames.Length>index) visual.sprite=frames[index]; }
    public void TakeDamage(float damage)
    {
        if(state==AnimationState.Spawn || state==AnimationState.Dead) return;
        Health=Mathf.Max(0,Health-Mathf.Max(0,damage));
        if(Health<=0) Die();
    }
    public void Die() { state=AnimationState.Dead; AudioManager.Instance?.SetMusic(false); Destroy(gameObject); }
    void OnDestroy() { foreach(var frames in new[]{spawn,walk,attack}) if(frames!=null) foreach(var sprite in frames) Destroy(sprite); }
}
