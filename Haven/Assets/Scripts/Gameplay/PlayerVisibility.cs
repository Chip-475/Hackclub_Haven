using UnityEngine;

// Screen-space darkness with radii measured in world units, independent of zoom.
public class PlayerVisibility : MonoBehaviour
{
    [Min(0)] public float clearRadius = 4.5f;
    [Min(.1f)] public float darkRadius = 9.5f;
    Texture2D gradient;
    void Awake()
    {
        const int size=256;
        gradient=new Texture2D(size,size,TextureFormat.RGBA32,false);
        gradient.wrapMode=TextureWrapMode.Clamp;
        var colors=new Color[size*size];
        float inner=Mathf.Clamp01(clearRadius/Mathf.Max(clearRadius+.1f,darkRadius));
        for(int y=0;y<size;y++) for(int x=0;x<size;x++)
        {
            float radius=Vector2.Distance(new Vector2(x,y),Vector2.one*((size-1)*.5f))/((size-1)*.5f);
            float alpha=Mathf.SmoothStep(0,1,Mathf.InverseLerp(inner,1,radius));
            colors[y*size+x]=new Color(.015f,.018f,.025f,alpha);
        }
        gradient.SetPixels(colors); gradient.Apply();
    }
    void OnGUI()
    {
        var player=PlayerManager.Instance; var camera=Camera.main;
        if(player==null || camera==null || gradient==null) return;
        Vector3 center=camera.WorldToScreenPoint(player.playerObj.transform.position+Vector3.up*.7f);
        float radius=darkRadius*Screen.height/(2*camera.orthographicSize);
        float left=center.x-radius, top=Screen.height-center.y-radius, diameter=radius*2;
        GUI.depth=-50; var color=GUI.color; GUI.color=Color.white;
        GUI.DrawTexture(new Rect(left,top,diameter,diameter),gradient);
        GUI.color=new Color(.015f,.018f,.025f,1);
        Fill(0,0,Screen.width,Mathf.Max(0,top));
        Fill(0,top+diameter,Screen.width,Mathf.Max(0,Screen.height-top-diameter));
        Fill(0,Mathf.Max(0,top),Mathf.Max(0,left),diameter);
        Fill(left+diameter,Mathf.Max(0,top),Mathf.Max(0,Screen.width-left-diameter),diameter);
        GUI.color=color;
    }
    static void Fill(float x,float y,float width,float height)
    {
        if(width>0 && height>0) GUI.DrawTexture(new Rect(x,y,width,height),Texture2D.whiteTexture);
    }
    void OnDestroy() { Destroy(gradient); }
}
