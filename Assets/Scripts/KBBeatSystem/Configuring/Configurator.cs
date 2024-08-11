using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KBBeat.Configuring
{
    internal class Configurator : MonoBehaviour 
    {
        public static Configurator Instance { get; private set; }
        public Configuration Config { get; private set; }

        private void Start() 
        {
            if (Instance == null) 
            {
                this.transform.parent = null;
                DontDestroyOnLoad(gameObject);
                Instance = this;
                this.Initialize();
            }
            else 
            {
                Destroy(gameObject);
            }
        }
        private async void Initialize() 
        {
            var fileInfo = new FileInfo(BuiltInSettings.ConfigurationPath);
            if (!fileInfo.Exists) 
            {
                this.Config = new();
                if (!fileInfo.Directory.Exists)
                {
                    fileInfo.Directory.Create();
                }
                using (var stream = fileInfo.Create())
                {
                    await WriteConfiguration(stream, this.Config);
                }
                return;
            }
            using (var stream = fileInfo.OpenRead())
            {
                try
                {
                    var reader = new StreamReader(stream);
                    this.Config = JsonUtility.FromJson<Configuration>(reader.ReadToEnd());
                    Debug.Log("Loaded configuration");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Error when loading configuration, using default: {e}");
                    this.Config = new();
                    await WriteConfiguration(stream, this.Config);
                }
            }
        }

        private async Task WriteConfiguration(FileStream stream, Configuration configuration) 
        {
            await stream.WriteAsync(Encoding.UTF8.GetBytes(JsonUtility.ToJson(configuration)));
            Debug.Log("Successfully written configuration");
        }

        public async Task Save() 
        {
            if (this.Config != null)
            {
                using (var stream = new FileStream(BuiltInSettings.ConfigurationPath, FileMode.Truncate))
                {
                    await this.WriteConfiguration(stream, this.Config);
                }
            }
        }
    }

    [Serializable]
    public class Configuration
    {
        [SerializeField] public float customOffsetSeconds = 0f;
        public string ToJson() 
        {
            return JsonUtility.ToJson(this);
        }

        public static Configuration FromJson(string json) 
        {
            return JsonUtility.FromJson<Configuration>(json);
        }
    }
}

