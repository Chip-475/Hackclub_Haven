using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioClip explorationMusic, bossMusic; // bossMusic is the victory cue, never the fight loop.
    [Range(0,1)] public float musicVolume = .3f, effectsVolume = .5f;
    AudioSource music, effects;
    Coroutine fade;
    AudioClip impact, bounce;
    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
        music = gameObject.AddComponent<AudioSource>(); music.loop = true;
        effects = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = effects.playOnAwake = false;
        impact = Tone(95, .12f); bounce = Tone(520, .09f);
    }
    AudioClip Tone(float frequency, float duration)
    {
        int count = (int)(44100 * duration); float[] samples = new float[count];
        for (int i=0;i<count;i++) samples[i] = Mathf.Sin(2*Mathf.PI*frequency*i/44100f)*Mathf.Pow(1-i/(float)count,2)*.3f;
        var clip = AudioClip.Create("Soft cue", count, 1, 44100, false); clip.SetData(samples,0); return clip;
    }
    public void PlayEffect(AudioClip clip) { if (clip != null) effects.PlayOneShot(clip, effectsVolume); }
    public void Impact() => PlayEffect(impact);
    public void Bounce() => PlayEffect(bounce);
    public void SetMusic(bool boss)
    {
        var clip = explorationMusic;
        if (clip == null || music.clip == clip) return;
        if (fade != null) StopCoroutine(fade);
        fade = StartCoroutine(SwitchMusic(clip));
    }
    public void PlayVictory()
    {
        if(bossMusic==null) return;
        if(fade!=null) StopCoroutine(fade);
        fade=StartCoroutine(SwitchMusic(bossMusic,47f,false));
    }
    IEnumerator SwitchMusic(AudioClip clip, float startSeconds=0, bool loop=true)
    {
        float volume = music.volume;
        for(float t=0;t<.5f;t+=Time.unscaledDeltaTime) { music.volume = Mathf.Lerp(volume,0,t/.5f); yield return null; }
        music.clip=clip; music.loop=loop; music.time=Mathf.Clamp(startSeconds,0,Mathf.Max(0,clip.length-.01f)); music.Play();
        for(float t=0;t<.8f;t+=Time.unscaledDeltaTime) { music.volume=musicVolume*t/.8f; yield return null; }
        music.volume=musicVolume;
    }
    void OnDestroy() { if(Instance==this) Instance=null; Destroy(impact); Destroy(bounce); }
}
