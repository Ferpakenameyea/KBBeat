using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; } = null;

    [SerializeField] private float musicVolume;

    [SerializeField] private float effectVolume;

    public delegate void VolumeChangeHandler(float oldVol, float newVol);

    public event VolumeChangeHandler OnMusicVolumeChange;

    public event VolumeChangeHandler OnEffectVolumeChange;

    [SerializeField] private List<AudioClip> soundTable;

    public float MusicVolume
    {
        get
        {
            return musicVolume;
        }

        set
        {
            this.OnMusicVolumeChange?.Invoke(musicVolume, value);
            this.musicVolume = value;
        }
    }

    public float EffectVolume
    {
        get
        {
            return effectVolume;
        }

        set
        {
            this.OnEffectVolumeChange?.Invoke(effectVolume, value);
            this.effectVolume = value;
        }
    }

    [SerializeField] private GameObject playerPoolPrefab;
    private ObjectPool playerPool;
    private List<AudioPlayer> activeEffectPlayers = new();
    private List<AudioPlayer> activeMusicPlayers = new();
    private Queue<AudioPlayer> toRemove = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            this.transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            this.playerPool = Instantiate(playerPoolPrefab, this.transform).GetComponent<ObjectPool>();
            this.OnEffectVolumeChange += (oldVol, newVol) =>
            {
                ChangeAllVolumesInList(activeEffectPlayers, newVol);
            };

            this.OnMusicVolumeChange += (oldVol, newVol) =>
            {
                ChangeAllVolumesInList(activeMusicPlayers, newVol);
            };

            StartCoroutine(ReturnPlayersCoroutine());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator ReturnPlayersCoroutine()
    {
        while (this.enabled)
        {
            yield return HandleList(activeMusicPlayers);
            yield return HandleList(activeEffectPlayers);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator HandleList(List<AudioPlayer> list)
    {
        foreach (var player in list)
        {
            if (player.Stopped)
            {
                toRemove.Enqueue(player);
            }
        }

        while (toRemove.Count > 0)
        {
            var removing = toRemove.Dequeue();
            list.Remove(removing);
            playerPool.Release(removing.gameObject);
        }

        yield return null;
    }

    private void ChangeAllVolumesInList(List<AudioPlayer> list, float newVolume)
    {
        foreach (var player in list)
        {
            player.Volume = newVolume;
        }
    }

    public AudioPlayer PlayClip(AudioClip clip, Channel channel)
    {
        return this.PlayClip(clip, 1.0f, channel, 1.0f, false);
    }

    public AudioPlayer PlayClip(AudioClip clip, float volume, Channel channel, float pitch = 1.0f, bool repeat = false)
    {
        var player = playerPool.Get(true).GetComponent<AudioPlayer>();
        player.transform.SetParent(this.transform);
        player.Repeat = repeat;
        player.Pitch = pitch;
        player.Clip = clip;

        switch (channel)
        {
            case Channel.Effect:
                player.Volume = volume * effectVolume;
                activeEffectPlayers.Add(player);
                break;
            case Channel.Music:
                player.Volume = volume * musicVolume;
                activeMusicPlayers.Add(player);
                break;

            default:
                player.Volume = volume;
                break;
        }
        player.Play();
        return player;
    }

    public void StopAllSounds()
    {
        foreach (var player in activeMusicPlayers)
        {
            player.Stop();
        }
        foreach (var player in activeEffectPlayers)
        {
            player.Stop();
        }
    }

    public void StopAllSounds(Channel channel)
    {
        var list = channel switch { Channel.Effect => this.activeEffectPlayers, Channel.Music => this.activeMusicPlayers, _ => null };
        if (list != null)
        {
            foreach (var item in list)
            {
                item.Stop();
            }
        }
    }

    public AudioPlayer PlayClip(string name, Channel channel)
    {
        var result = this.soundTable.Find((s) => s.name.Equals(name));
        if (result != null)
        {
            return this.PlayClip(result, channel);
        }
        return null;
    }
}

public enum Channel
{
    Effect,
    Music
}