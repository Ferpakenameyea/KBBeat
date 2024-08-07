using System;
using System.IO;
using System.Text;
using UnityEngine;

public class LoggingManager : MonoBehaviour
{
    private static FileStream FileWriter;
    private static UTF8Encoding encoding;
    private static LoggingManager Instance = null;

    private void Awake()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor)
        {
            Destroy(gameObject);
            return;
        }

        if (Instance == null)
        {
            Instance = this;
            this.transform.parent = null;
            DontDestroyOnLoad(gameObject);
            this.ClearLogs();
            this.LogStart();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ClearLogs()
    {
        var dir = new DirectoryInfo(BuiltInSettings.LoggingSavePath);
        if (!dir.Exists)
        {
            return;
        }
        var files = dir.GetFiles();
        if (files.Length < BuiltInSettings.MaxLoggingCount)
        {
            return;
        }
        var toDelete = files.Length - (BuiltInSettings.MaxLoggingCount - 1);
        Array.Sort(files, (f1, f2) =>
        {
            return f1.CreationTime.CompareTo(f2.CreationTime);
        });

        for (int i = 0; i < toDelete; i++)
        {
            files[i].Delete();
        }
    }

    private async void PrintBannerAsync()
    {
        string banner = null;
        try
        {
            banner = Resources.Load<TextAsset>("Banner").text;
        }
        catch (NullReferenceException)
        {
            // no banner found
            return;
        }
        await FileWriter.WriteAsync(encoding.GetBytes(banner + "\n\n"));
        await FileWriter.FlushAsync();
    }

    private void LogStart()
    {
        var directory = new DirectoryInfo(BuiltInSettings.LoggingSavePath);
        if (!directory.Exists)
        {
            directory.Create();
        }
        string NowTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss").Replace(" ", "_").Replace("/", "_").Replace(":", "_");
        FileInfo fileInfo = new FileInfo(BuiltInSettings.LoggingSavePath + "/" + NowTime + "_Log.txt");

        FileWriter = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
        encoding = new UTF8Encoding();
        this.PrintBannerAsync();
        Application.logMessageReceived += LogCallbackAsync;
    }

    /// <summary>
    /// Log回调
    /// </summary>
    /// <param name="condition">输出内容</param>
    /// <param name="stackTrace">堆栈追踪</param>
    /// <param name="type">Log日志类型</param>
    private async void LogCallbackAsync(
        string condition,
        string stackTrace,
        LogType type) //写入控制台数据
    {
        //输出的日志类型可以自定义
        string content =
        $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}[{type}][{stackTrace}]:{condition}\n\n";
        await FileWriter.WriteAsync(encoding.GetBytes(content), 0, encoding.GetByteCount(content));
        await FileWriter.FlushAsync();
    }

    private void OnDestroy() //关闭写入
    {
        if ((FileWriter != null))
        {
            Application.logMessageReceived -= LogCallbackAsync;
            FileWriter.WriteAsync(encoding.GetBytes("Logging closing"))
                .GetAwaiter()
                .OnCompleted(() =>
                {
                    FileWriter.Flush();
                    FileWriter.Close();
                });
        }
    }

    private void OnDisable() => OnDestroy();
}