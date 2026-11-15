using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Runs against the real scene and physics in the user's licensed Editor.
[InitializeOnLoad]
public static class GameplaySmokeTest
{
    const string Key = "Haven.GameplaySmokeTest";
    static int stage;
    static double deadline;
    static Vector3 bossPosition;
    static float bossHealth;
    static GameplaySmokeTest()
    {
        EditorApplication.playModeStateChanged += mode =>
        {
            if(mode==PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Key,false))
            { stage=0; deadline=EditorApplication.timeSinceStartup+2; EditorApplication.update+=Tick; }
        };
    }
    [MenuItem("Haven/Tests/Run gameplay smoke test")]
    public static void Run()
    {
        if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before running the smoke test.");
        if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        SessionState.SetBool(Key,true); EditorApplication.isPlaying=true;
    }
    static void Check(bool condition,string message) { if(!condition) throw new Exception(message); }
    static void Tick()
    {
        if(!EditorApplication.isPlaying) { EditorApplication.update-=Tick; SessionState.SetBool(Key,false); return; }
        if(EditorApplication.timeSinceStartup<deadline) return;
        try
        {
            var player=PlayerManager.Instance;
            Check(player!=null,"Player missing");
            var director=GameplayDirector.Instance;
            if(stage==0)
            {
                Check(director!=null,"Director missing");
                Check(UnityEngine.Object.FindObjectsByType<Swordsman>().Length==2,"Expected two guards");
                Check(UnityEngine.Object.FindAnyObjectByType<PlayerVisibility>()!=null,"Fog missing");
                Check(AudioManager.Instance!=null,"Audio manager missing");
                var box=player.playerObj.GetComponent<BoxCollider2D>();
                Vector2 size=box.size* .98f;
                var hits=Physics2D.BoxCastAll(new Vector2(0,-3.45f+size.y/2),size,0,Vector2.right,49,player.groundLayer);
                Check(hits.Length==0,"A ground collider blocks the ground route to the arena");
                foreach(var guard in UnityEngine.Object.FindObjectsByType<Swordsman>()) guard.enabled=false;
                player.playerObj.transform.position=new Vector3(36,-2.95f,0);
                Physics2D.SyncTransforms();
                stage=1; deadline=EditorApplication.timeSinceStartup+1.7;
            }
            else if(stage==1)
            {
                var boss=UnityEngine.Object.FindAnyObjectByType<BossController>();
                Check(boss!=null && boss.state!=BossController.AnimationState.Spawn,"Boss failed to spawn and finish introduction");
                Check(boss.GetComponent<SpriteRenderer>().sprite!=null,"Boss sprites missing");
                bossHealth=boss.Health; bossPosition=boss.transform.position;
                player.Die();
                Check(player.IsRespawning && Time.timeScale==0,"Bounceback did not freeze the world");
                stage=2; deadline=EditorApplication.timeSinceStartup+.8;
            }
            else if(stage==2)
            {
                var boss=UnityEngine.Object.FindAnyObjectByType<BossController>();
                Check(boss!=null && boss.Health==bossHealth && Vector3.Distance(bossPosition,boss.transform.position)<.01f,"World changed during Bounceback");
                stage=3; deadline=EditorApplication.timeSinceStartup+2;
            }
            else if(stage==3)
            {
                var boss=UnityEngine.Object.FindAnyObjectByType<BossController>();
                Check(!player.IsRespawning && Time.timeScale==1 && player.Health==player.playerData.maxHp,"Respawn failed");
                Check(Vector3.Distance(player.playerObj.transform.position,director.RespawnPoint)<.2f,"Unsafe checkpoint");
                Check(boss!=null && boss.Health==bossHealth,"Enemy health changed during respawn");
                boss.TakeDamage(1000);
                stage=4; deadline=EditorApplication.timeSinceStartup+.2;
            }
            else
            {
                Check(UnityEngine.Object.FindAnyObjectByType<BossController>()==null,"Boss death failed");
                Debug.Log("HAVEN SMOKE TEST PASSED: route, guards, fog, audio, boss intro, Bounceback, world preservation, boss death.");
                Finish();
            }
        }
        catch(Exception error) { Debug.LogError("HAVEN SMOKE TEST FAILED: "+error.Message); Finish(); }
    }
    static void Finish()
    {
        EditorApplication.update-=Tick; SessionState.SetBool(Key,false); EditorApplication.isPlaying=false;
    }
}
