using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour, IDamageable
{
    public enum AnimationState { Spawn, Idle, Walking, Attack, Dead, Dialogue, Waiting, Fracture }
    public AnimationState state;
    public float Health { get; private set; } = 35f;
    public float MaxHealth => 35f;
    public float Defense => 0;
    SpriteRenderer visual;
    Sprite[] spawn, walk, attack;
    float clock, cooldown;
    bool hit;
    int returns;
    float fightTime, hurtUntil;
    BossDialogue dialogue;
    public int ReturnCount => returns;
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
        dialogue=gameObject.AddComponent<BossDialogue>();
        visual=gameObject.AddComponent<SpriteRenderer>(); visual.sortingOrder=5;
        spawn=Frames("Spawn",5); walk=Frames("Walk",4); attack=Frames("Attack",3);
        var collider=gameObject.AddComponent<BoxCollider2D>(); collider.size=new Vector2(1.2f,3.7f); collider.offset=new Vector2(0,1.85f);
        gameObject.AddComponent<EnemyPresentation>();
        gameObject.tag="Enemy"; gameObject.layer=LayerMask.NameToLayer("Enemy");
    }
    IEnumerator Start() { yield return Introduction(); }
    IEnumerator Introduction()
    {
        state=AnimationState.Dialogue;
        Show(spawn,0); visual.flipX=true;
        var player=PlayerManager.Instance;
        player.SetCinematicLock(true);
        yield return dialogue.Play(BossDialogue.Encounters[Mathf.Min(returns,BossDialogue.Encounters.Length-1)]);
        state=AnimationState.Spawn;
        for(int i=0;i<spawn.Length;i++)
        {
            visual.sprite=spawn[i];
            if(i==1 || i==2) { GameplayDirector.Instance.Impact(); yield return new WaitForSeconds(.07f); }
            yield return new WaitForSeconds(i==0?.25f:.16f);
        }
        fightTime=0; clock=0; cooldown=Time.time+.65f;
        state=AnimationState.Idle;
        player.SetCinematicLock(false);
    }
    public void PlayerDied()
    {
        if(state==AnimationState.Dead || state==AnimationState.Fracture || state==AnimationState.Waiting) return;
        if(PlayerManager.Instance.playerObj.transform.position.x<32) return;
        returns++;
        StopAllCoroutines(); dialogue.Cancel();
        PlayerManager.Instance.SetCinematicLock(false);
        state=AnimationState.Waiting; Show(spawn,0); visual.color=Color.white;
    }
    void Update()
    {
        if(state==AnimationState.Spawn || state==AnimationState.Dialogue || state==AnimationState.Fracture || state==AnimationState.Dead || Time.timeScale==0) return;
        var player=PlayerManager.Instance;
        if(player==null || player.IsRespawning) return;
        if(state==AnimationState.Waiting)
        {
            if(player.playerObj.transform.position.x>32) StartCoroutine(Introduction());
            return;
        }
        visual.color=Time.time<hurtUntil?new Color(1,.45f,.35f):Color.white;
        fightTime+=Time.deltaTime;
        if(returns>=5 && fightTime>=10f) { Die(); return; }
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
            if(clock>=.8f) { state=AnimationState.Idle; cooldown=Time.time+Mathf.Max(.65f,1.2f-returns*.1f); }
            return;
        }
        facing=dx<0?-1:1; visual.flipX=facing<0;
        if(player.playerObj.transform.position.x<32) { state=AnimationState.Idle; Show(walk,0); return; }
        if(Mathf.Abs(dx)>2.5f)
        {
            state=AnimationState.Walking; clock+=Time.deltaTime;
            transform.position=new Vector3(Mathf.Clamp(transform.position.x+facing*(1.7f+Mathf.Min(returns,5)*.12f)*Time.deltaTime,34,48),transform.position.y,0);
            Show(walk,(int)(clock*6)%4);
        }
        else if(Time.time>=cooldown) { state=AnimationState.Attack; clock=0; hit=false; }
        else { state=AnimationState.Idle; Show(walk,0); }
    }
    void Show(Sprite[] frames,int index) { if(frames.Length>index) visual.sprite=frames[index]; }
    public void TakeDamage(float damage)
    {
        if(damage<=0 || state==AnimationState.Spawn || state==AnimationState.Dialogue || state==AnimationState.Waiting || state==AnimationState.Fracture || state==AnimationState.Dead) return;
        Health=Mathf.Max(1,Health-damage);
        hurtUntil=Time.time+.1f; GameplayDirector.Instance?.HitFeedback();
        if(Health<=1) Die();
    }
    public void Die()
    {
        if(state==AnimationState.Fracture || state==AnimationState.Dead) return;
        StopAllCoroutines(); dialogue.Cancel();
        state=AnimationState.Fracture;
        PlayerManager.Instance.SetCinematicLock(false);
        StartCoroutine(Fracture());
    }
    IEnumerator Fracture()
    {
        Show(spawn,0); visual.color=Color.white;
        yield return dialogue.Play(BossDialogue.Ending);
        float remaining=Health;
        Vector3 origin=transform.position;
        for(float t=0;t<3f;t+=Time.deltaTime)
        {
            float u=t/3f;
            Health=Mathf.Lerp(remaining,0,u);
            visual.color=new Color(.65f+.35f*(1-u),.7f,.9f,1-Mathf.SmoothStep(0,1,u));
            transform.position=origin+new Vector3(Mathf.Sin(t*38)*.025f*u,u*.35f,0);
            yield return null;
        }
        Health=0; state=AnimationState.Dead;
        GameplayDirector.Instance?.HitFeedback(.045f);
        AudioManager.Instance?.PlayVictory();
        Destroy(gameObject);
    }
    void OnDestroy() { foreach(var frames in new[]{spawn,walk,attack}) if(frames!=null) foreach(var sprite in frames) Destroy(sprite); }
}
