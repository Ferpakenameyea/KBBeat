using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KBBeat.Common;
using UnityEngine;

internal class ScoreCounterLoader : MonoBehaviour
{
    private void Start()
    {
        ScoreRecorder.Initialize();
        Destroy(gameObject);
    }
}

namespace KBBeat.Common
{
    public static class ScoreRecorder
    {
        private static Dictionary<string, LevelHighScoreRecord> scoreMap;
        internal static void Initialize()
        {
            var file = new FileInfo(BuiltInSettings.PlayerFilePath);
            if (!file.Directory.Exists)
            {
                file.Directory.Create();
            }
            if (!file.Exists)
            {
                Debug.LogWarningFormat("player file {0} not found, creating new", BuiltInSettings.PlayerFilePath);
                file.Create().Close();
                scoreMap = new();
                return;
            }
            var reader = new StreamReader(file.OpenRead());
            var json = reader.ReadToEnd();
            try
            {
                scoreMap = DictionarySerializer<string, LevelHighScoreRecord>.FromJson(json);
                if (scoreMap == null)
                {
                    throw new Exception("Illegal json");
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load high score json, recording might be discarded");
                Debug.LogException(e);
                scoreMap = new();
            }
            finally
            {
                reader?.Close();
            }
        }

        private static async void SaveAsync()
        {
            var file = new FileInfo(BuiltInSettings.PlayerFilePath);
            if (file.Exists)
            {
                file.Delete();
            }
            var writer = new StreamWriter(file.Create());
            try
            {

                var json = await Task.Run<string>(() =>
                    DictionarySerializer<string, LevelHighScoreRecord>.ToJson(scoreMap)
                );

                await writer.WriteAsync(json);
                Debug.Log("Saved");
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Error when writing player file");
                Debug.LogException(e);
            }
            finally
            {
                writer.Close();
            }
            return;
        }

        internal static LevelHighScoreRecord GetRecord(string levelAssetBundle)
        {
            if (scoreMap.TryGetValue(levelAssetBundle, out var res))
            {
                return res;
            }
            return null;
        }

        internal static bool Record(string levelNameAssetBundle, LevelHighScoreRecord score)
        {
            if (scoreMap.TryGetValue(levelNameAssetBundle, out var old))
            {
                if (score.highAcc <= old.highAcc && score.highScore <= old.highScore)
                {
                    return false;
                }

                scoreMap.Remove(levelNameAssetBundle);
            }

            scoreMap.Add(levelNameAssetBundle, score);
            SaveAsync();
            return true;
        }

        public static void Clear()
        {
            scoreMap.Clear();
            var f = new FileInfo(BuiltInSettings.PlayerFilePath);
            if (f.Exists)
            {
                f.Delete();
            }
            return;
        }
    }

    [Serializable]
    internal class LevelHighScoreRecord
    {
        [SerializeField] public int highScore;
        [SerializeField] public float highAcc;
        [SerializeField] public bool allCute;

        public LevelHighScoreRecord(int highScore, float highAcc, bool allCute)
        {
            this.highScore = highScore;
            this.highAcc = highAcc;
            this.allCute = allCute;
        }
    }

    [Serializable]
    internal struct DictionaryEntry<TKey, TValue>
    {
        public TKey key;
        public TValue value;

        public DictionaryEntry(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }

    internal static class DictionarySerializer<TKey, TValue>
    {
        public static string ToJson(Dictionary<TKey, TValue> target)
        {
            var list = new List<DictionaryEntry<TKey, TValue>>();
            foreach (var entry in target)
            {
                list.Add(new(entry.Key, entry.Value));
            }
            return JsonUtility.ToJson(
                new JsonWrapper<List<DictionaryEntry<TKey, TValue>>>(list)
            );
        }

        public static Dictionary<TKey, TValue> FromJson(string json)
        {
            var list = JsonUtility.FromJson<JsonWrapper<List<DictionaryEntry<TKey, TValue>>>>(json).data;
            var dict = new Dictionary<TKey, TValue>();
            foreach (var entry in list)
            {
                dict.Add(entry.key, entry.value);
            }
            return dict;
        }
    }

    [Serializable]
    internal struct JsonWrapper<T>
    {
        [SerializeField] public T data;

        public JsonWrapper(T data)
        {
            this.data = data;
        }
    }
}
