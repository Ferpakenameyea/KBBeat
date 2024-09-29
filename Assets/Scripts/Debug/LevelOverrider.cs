using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Unity.Profiling;
using UnityEngine;

public class LevelOverrider : MonoBehaviour
{
    public static LevelOverrider Instance { get; private set; } = null;
    public static bool Enabled { get => Instance != null; }

    private void Awake() 
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public InPlayingEnvironment GetOverrider() 
    {
        FileInfo fileInfo = new FileInfo("./Debug/Override/inPlaying.json");
        
        if (!fileInfo.Directory.Exists) 
        {
            fileInfo.Directory.Create();
        }

        if (!fileInfo.Exists) 
        {
            return null;
        }

        try
        {
            using (var stream = new StreamReader(fileInfo.OpenRead())) 
            {
                var json = stream.ReadToEnd();
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new InPlayingEnvironment.Note.NoteJsonConverter());
                var inPlaying = JsonConvert.DeserializeObject<InPlayingEnvironment>(
                    json, settings
                );

                Debug.Log("Found global inplaying");
                return inPlaying;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error loading overrider script");
            Debug.LogException(e);
            return null;            
        }
    }
}
