using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour 
{
    [SerializeField] private Explosion explosion;
    public void TestClip()
    {
        var clip = Resources.Load("cyber") as AudioClip;
        SoundManager.Instance.PlayClip(clip, Channel.Music);
    }

    public void TestLevel()
    {
        LevelManager.Instance.LoadLevelAsync("level0", encoding: System.Text.Encoding.UTF8);
        LevelManager.Instance.OnLoadLevelSuccess += (name) =>
        {
            LevelPlayer.Instance.PlayLevel(LevelManager.Instance.LoadedLevel);
        };
    }

    public void TestExplosion()
    {
        var _ = Instantiate(explosion);
    }
}
