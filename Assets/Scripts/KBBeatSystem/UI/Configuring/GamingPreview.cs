using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using KBBeat.Audio;
using KBBeat.Common;
using KBBeat.Configuring;
using KBBeat.Core;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using XLua.LuaDLL;
using static KBBeat.Core.InPlayingEnvironment;

internal class GamingPreview : MonoBehaviour
{
    [SerializeField] private GameObject previewNoteObjectPrefab;
    [SerializeField] private AudioClip clip;
    [SerializeField] private TextAsset noteSheet;
    [SerializeField] private Vector3 appear;
    [SerializeField] private Track track;

    private float passed;

    #region SHINE
    private Queue<PreviewNoteObject> previewNoteObjects = new();
    [SerializeField] private GameObject shineObject;
    [SerializeField] private Vector3 shineOffset;
    #endregion

    private ObjectPool<PreviewNoteObject> notePool;
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
        var field = typeof(PreviewNoteObject).GetField("pool", BindingFlags.Instance | BindingFlags.NonPublic);

        this.notePool = new(
            () => {
                var pre = Instantiate(this.previewNoteObjectPrefab).GetComponent<PreviewNoteObject>();
                field.SetValue(pre, this.notePool);
                return pre;
            },
            o => o.gameObject.SetActive(true),
            o => o.gameObject.SetActive(false),
            o => Destroy(o.gameObject),
            defaultCapacity: 5,
            maxSize: 20
        );

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
                passed = Time.time - startTime;
                if (passed > notes.Peek().StrikeTime - BuiltInSettings.MoveTime + this.Offset)
                {
                    var note = notes.Dequeue();
                    var previewObject = this.notePool.Get().GetComponent<PreviewNoteObject>();
                    previewObject.Launch(note, startTime, this.appear, this.track);
                    launched++;
                    previewNoteObjects.Enqueue(previewObject);
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

    private void Update() 
    {
        if (!previewNoteObjects.Any())
        {
            return;
        }

        var head = previewNoteObjects.First();
        Debug.LogFormat("Any, passed={0}, time is={1}", passed, head.StrikeTime + this.Offset);
        if (head.gameObject.activeInHierarchy == false)
        {
            Debug.Log("Dequeue");
            _ = previewNoteObjects.Dequeue();
        }

        if (Input.anyKeyDown)
        {
            var shine = Instantiate(this.shineObject);
            shine.transform.SetParent(head.transform);
            shine.transform.SetLocalPositionAndRotation(
                this.shineOffset, 
                Quaternion.identity);
            shine.transform.SetParent(head.transform.parent);
        }
    }

    private void OnDestroy() 
    {
        this.notePool?.Dispose();        
    }
}
