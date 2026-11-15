using System.Collections;
using UnityEngine;

public class GameplayDirector : MonoBehaviour
{
    public static GameplayDirector Instance { get; private set; }
    public AudioClip explorationMusic, bossMusic;
    public Sprite arenaBackdrop;
    public bool IsBouncing { get; private set; }
    public float Shake { get; private set; }
    public Vector3 RespawnPoint { get; private set; } = new Vector3(-5,-2.95f,0);
    float darkness, flash, previousScale=1;
    Vector2 orb;
    bool showOrb, bossStarted;
    Texture2D circle;
    void Awake() { Instance=this; }
    void Start()
    {
        gameObject.AddComponent<PlayerVisibility>();
        var audio=gameObject.AddComponent<AudioManager>();
        audio.explorationMusic=explorationMusic; audio.bossMusic=bossMusic; audio.SetMusic(false);
        var guard=FindAnyObjectByType<Swordsman>();
        if(guard!=null)
        {
            guard.transform.position=new Vector3(5,-2.95f,0); guard.name="Guard - Atrium"; guard.PatrolBounds=new Vector2(-6,7);
            var second=Instantiate(guard,new Vector3(23,-2.95f,0),Quaternion.identity); second.name="Guard - Gallery"; second.PatrolBounds=new Vector2(13,27);
        }
        if(arenaBackdrop!=null)
        {
            var backdrop=new GameObject("Boss arena backdrop"); var sr=backdrop.AddComponent<SpriteRenderer>(); sr.sprite=arenaBackdrop; sr.sortingOrder=-2;
            backdrop.transform.position=new Vector3(41,1.55f,0);
            backdrop.transform.localScale=Vector3.one*(18f/arenaBackdrop.bounds.size.x);
        }
        circle=new Texture2D(64,64,TextureFormat.RGBA32,false);
        for(int y=0;y<64;y++) for(int x=0;x<64;x++) circle.SetPixel(x,y,new Color(.12f,.65f,1,Mathf.Clamp01(31-Vector2.Distance(new Vector2(x,y),new Vector2(31.5f,31.5f)))));
        circle.Apply();
    }
    void Update()
    {
        flash=Mathf.MoveTowards(flash,0,Time.unscaledDeltaTime*9);
        Shake=Mathf.MoveTowards(Shake,0,Time.unscaledDeltaTime*.7f);
        var player=PlayerManager.Instance;
        if(player==null || IsBouncing) return;
        float x=player.playerObj.transform.position.x;
        if(!bossStarted && x>32)
        {
            bossStarted=true;
            var boss=new GameObject("Halo Boss"); boss.transform.position=new Vector3(39,-3.5f,0); boss.AddComponent<BossController>();
            AudioManager.Instance?.SetMusic(true);
        }
        if(player.playerObj.transform.position.y<-12) player.Die();
    }
    public void Impact() { if(IsBouncing) return; flash=.9f; Shake=.08f; AudioManager.Instance?.Impact(); }
    public void HitFeedback(float strength=.035f)
    {
        if(!IsBouncing) Shake=Mathf.Max(Shake,strength);
    }
    public void Bounceback(PlayerManager player)
    {
        if(IsBouncing) return;
        FindAnyObjectByType<BossController>()?.PlayerDied();
        StartCoroutine(Respawn(player));
    }
    IEnumerator Respawn(PlayerManager player)
    {
        IsBouncing=true; previousScale=Time.timeScale; Time.timeScale=0;
        var camera=Camera.main;
        Vector3 origin=camera.WorldToViewportPoint(player.playerObj.transform.position);
        orb=new Vector2(Mathf.Clamp01(origin.x),1-Mathf.Clamp01(origin.y));
        for(float t=0;t<.2f;t+=Time.unscaledDeltaTime) { darkness=t/.2f; yield return null; }
        darkness=1; showOrb=true;
        var renders=player.playerObj.GetComponentsInChildren<SpriteRenderer>();
        bool[] visible=new bool[renders.Length];
        for(int i=0;i<renders.Length;i++) { visible[i]=renders[i].enabled; renders[i].enabled=false; }
        Vector3 start=player.playerObj.transform.position;
        Vector3 cameraStart=camera.transform.position;
        float startingZoom=camera.orthographicSize;
        int bounces=Mathf.Max(2,Mathf.CeilToInt(Vector2.Distance(start,RespawnPoint)/5f));
        float duration=bounces*.48f;
        int lastBounce=-1;
        for(float t=0;t<duration;t+=Time.unscaledDeltaTime)
        {
            float u=Mathf.Clamp01(t/duration);
            float travel=Mathf.SmoothStep(0,1,u);
            Vector3 ball=Vector3.Lerp(start,RespawnPoint,travel);
            ball.y+=Mathf.Abs(Mathf.Sin(u*Mathf.PI*bounces))*.9f;
            Vector3 baseline=Vector3.Lerp(start,RespawnPoint,travel);
            Vector3 follow=new Vector3(baseline.x,baseline.y,cameraStart.z);
            camera.transform.position=Vector3.Lerp(cameraStart,follow,Mathf.SmoothStep(0,1,Mathf.Min(u*4,1)));
            camera.orthographicSize=Mathf.Lerp(startingZoom,5,travel);
            Vector3 screen=camera.WorldToViewportPoint(ball);
            orb=new Vector2(screen.x,1-screen.y);
            int bounce=(int)(u*bounces);
            if(bounce!=lastBounce) { AudioManager.Instance?.Bounce(); lastBounce=bounce; }
            yield return null;
        }
        orb=new Vector2(.5f,.5f);
        camera.transform.position=new Vector3(RespawnPoint.x,RespawnPoint.y,cameraStart.z);
        camera.orthographicSize=5;
        player.RestoreAfterDeath(RespawnPoint);
        for(int i=0;i<renders.Length;i++) renders[i].enabled=visible[i];
        showOrb=false;
        for(float t=0;t<.4f;t+=Time.unscaledDeltaTime) { darkness=1-t/.4f; yield return null; }
        darkness=0; IsBouncing=false; Time.timeScale=previousScale;
        player.ResumeAfterDeath();
    }
    void OnGUI()
    {
        GUI.depth=-100; Color old=GUI.color;
        if(darkness>0) { GUI.color=new Color(0,0,0,darkness); GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture); }
        if(showOrb && circle!=null) { GUI.color=Color.white; float size=Screen.height*.035f; GUI.DrawTexture(new Rect(orb.x*Screen.width-size/2,orb.y*Screen.height-size/2,size,size),circle); }
        if(flash>0 && !IsBouncing) { GUI.color=new Color(1,1,1,flash); GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture); }
        GUI.color=old;
    }
    void OnDestroy() { if(IsBouncing) Time.timeScale=previousScale; if(Instance==this) Instance=null; Destroy(circle); }
}
