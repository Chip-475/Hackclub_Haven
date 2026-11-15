using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossDialogue : MonoBehaviour
{
    public bool IsOpen { get; private set; }
    string speaker, line="";
    int letters;
    float opacity;
    bool advance;
    GUIStyle textStyle, nameStyle, hintStyle;
    // Each return advances the relationship, rather than repeating a random taunt.
    public static readonly string[][] Encounters = {
        new[]{"SPIRITO|Sei uno dei rinati? Fammi divertire.", "RAGAZZO|Mi hanno dato a te. Non ti hanno dato il mio nome.", "SPIRITO|Allora terrò il conto delle tue morti."},
        new[]{"SPIRITO|Tornata? Mi stai facendo divertire!", "RAGAZZO|È la mia anima che torna. Il dolore conosce già la strada.", "SPIRITO|Vediamo quanto ricorda la carne."},
        new[]{"SPIRITO|Prima del tuo passo, sento il silenzio cambiare. Ti aspettavo.", "RAGAZZO|Curavo le ferite degli altri. Nessuno si fermava sulle mie.", "SPIRITO|Io ricordo dove le ho aperte. Tutte."},
        new[]{"SPIRITO|Conosco la tua schivata. Eppure ogni ritorno mi trova diverso.", "RAGAZZO|Restauro ciò che ero. Non posso restituirti ciò che eri tu.", "SPIRITO|Taci. Qui sono io a decidere che cosa rimane."},
        new[]{"SPIRITO|Dovrei desiderare il tuo grido. Perché ascolto il tuo respiro?", "RAGAZZO|La tua forza era una certezza. Ora le tremano le mani.", "SPIRITO|Non chiamarla pietà. Non ho un nome per questo."},
        new[]{"SPIRITO|Ho imparato ogni tuo movimento. Ho dimenticato perché ti colpisco.", "RAGAZZO|Non sono tornato più forte. Sono soltanto tornato.", "SPIRITO|E io non riesco più a essere quello che ti aspettava."}
    };
    public static readonly string[] Ending = {
        "SPIRITO|I have killed you more times than I can remember.",
        "SPIRITO|No. That's a lie.",
        "SPIRITO|I remember them all."
    };
    public void Advance() => advance=true;
    void Update()
    {
        if(IsOpen && Keyboard.current!=null && (Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)) Advance();
    }
    public IEnumerator Play(string[] lines)
    {
        IsOpen=true; opacity=0;
        foreach(string entry in lines)
        {
            int divider=entry.IndexOf('|'); speaker=entry.Substring(0,divider); line=entry.Substring(divider+1);
            letters=0; advance=false; float clock=0;
            while(letters<line.Length)
            {
                opacity=Mathf.MoveTowards(opacity,1,Time.unscaledDeltaTime*5);
                clock+=Time.unscaledDeltaTime*34; letters=Mathf.Min(line.Length,(int)clock);
                if(advance) { letters=line.Length; advance=false; }
                yield return null;
            }
            while(!advance) { opacity=Mathf.MoveTowards(opacity,1,Time.unscaledDeltaTime*5); yield return null; }
            advance=false;
        }
        while(opacity>0) { opacity=Mathf.MoveTowards(opacity,0,Time.unscaledDeltaTime*5); yield return null; }
        IsOpen=false;
    }
    public void Cancel() { IsOpen=false; opacity=0; advance=false; }
    void OnGUI()
    {
        if(!IsOpen || opacity<=0) return;
        if(textStyle==null)
        {
            textStyle=new GUIStyle(GUI.skin.label){wordWrap=true,richText=false,fontSize=23};
            nameStyle=new GUIStyle(GUI.skin.label){fontSize=13,fontStyle=FontStyle.Bold};
            hintStyle=new GUIStyle(GUI.skin.label){fontSize=12,alignment=TextAnchor.LowerRight};
        }
        GUI.depth=-80; Color old=GUI.color;
        float width=Mathf.Min(850,Screen.width*.86f), height=Mathf.Min(190,Screen.height*.3f);
        Rect box=new Rect((Screen.width-width)/2,Screen.height-height-30,width,height);
        GUI.color=new Color(.025f,.02f,.025f,opacity*.95f); GUI.DrawTexture(box,Texture2D.whiteTexture);
        GUI.color=new Color(.7f,.3f,.2f,opacity); GUI.DrawTexture(new Rect(box.x,box.y,3,box.height),Texture2D.whiteTexture);
        GUI.Label(new Rect(box.x+24,box.y+14,width-48,22),speaker,nameStyle);
        GUI.color=new Color(.95f,.9f,.81f,opacity);
        textStyle.fontSize=Mathf.Clamp(Screen.width/55,16,23);
        GUI.Label(new Rect(box.x+24,box.y+43,width-48,height-75),line.Substring(0,letters),textStyle);
        GUI.Label(new Rect(box.x+24,box.yMax-27,width-48,20),"E / Spazio / Invio · continua",hintStyle);
        GUI.color=old;
    }
}
