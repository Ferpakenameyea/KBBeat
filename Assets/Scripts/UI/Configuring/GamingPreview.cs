using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using static InPlayingEnvironment;

public class GamingPreview : MonoBehaviour
{
    [SerializeField] private ObjectPool notePool;
    [SerializeField] private AudioClip clip;
    [SerializeField] private TextAsset noteSheet;
    [SerializeField] private Vector3 appear;
    [SerializeField] private Track track;

    private AudioPlayer player;
    private List<Note> notes;
    
    private float Offset
    {
        get
        {
            return BuiltInSettings.systemOffsetSeconds + 
                Configurator.Instance.Config.customOffsetSeconds;
        }
    }

    private void OnDisable()
    {
        if (this.player != null)
        {
            this.player.Stop();
        }
    }


    private void Start()
    {
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(new Note.NoteJsonConverter());

        this.notes = JsonConvert.DeserializeObject<JsonWrapper<List<Note>>>(this.noteSheet.text, settings).data;

        Queue<Note> notes = new Queue<Note>(this.notes);

        IEnumerator PreviewCoroutine()
        {
            float startTime = Time.time;
            int launched = 0;
            this.player = SoundManager.Instance.PlayClip(this.clip, 1.0f, Channel.Music);
            while(launched < this.notes.Count)
            {
                float passed = Time.time - startTime;
                if (passed > notes.Peek().StrikeTime - BuiltInSettings.MoveTime + this.Offset)
                {
                    var note = notes.Dequeue();
                    this.notePool.Get(true).GetComponent<PreviewNoteObject>()
                        .Launch(note, startTime, this.appear, this.track);
                    launched++;
                    notes.Enqueue(note);
                }
                yield return null;
            }
            yield return new WaitUntil(() => this.player.Stopped);
            StartCoroutine(PreviewCoroutine());
            yield break;
        }
        StartCoroutine(PreviewCoroutine());
    }
}
