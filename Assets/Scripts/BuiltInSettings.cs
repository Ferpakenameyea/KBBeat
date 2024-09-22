using KBbeat;
using System;
using System.IO;
using UnityEngine;

public static class BuiltInSettings
{
    private static float moveTime = 1.6f;
    private static bool gamePaused = false;
    private static float expectedDelay = 2.0f;
    public const int recordingFrames = 10;
    public const float missLine = 0.080f;
    public const float goodLine = 0.050f;
    public const float tooEarlyLine = 0.080f;
    public const float systemOffsetSeconds = 0f;
    public const float holdEndOffset = 0.080f;
    public static float CustomOffsetSeconds
    {
        get
        {
            if (Configurator.Instance == null)
            {
                return 0f;
            }
            return Configurator.Instance.Config.customOffsetSeconds;
        }
    }
    public const float farLine = 0.100f;
    public const int PowerRange = 1;
    public const bool Debug = false;
    public static string PlayerFilePath
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "./player/player.json";
                default:
                    return Path.Combine(Application.persistentDataPath, "player/player.json");
            }
        }
    }
    public static event Action OnGamePause;
    public static event Action OnGameResume;
    public static event Action<float> OnMoveTimeChange;
    public const float epsilon = 0.00001f;
    public const int ResumeCountdown = 3;
    public static bool GamePaused
    {
        get
        {
            return gamePaused;
        }
        set
        {
            if (gamePaused != value)
            {
                gamePaused = value;
                if (value == true)
                {
                    OnGamePause?.Invoke();
                }
                else
                {
                    OnGameResume?.Invoke();
                }
            }
        }
    }
    public static float MoveTime
    {
        get
        {
            return moveTime;
        }
        set
        {
            moveTime = value;
            OnMoveTimeChange?.Invoke(value);
        }
    }
    public static float ExpectedDelayTime
    {
        get
        {
            return expectedDelay;
        }
        set
        {
            if (LevelPlayer.Instance.IsPlaying)
            {
                UnityEngine.Debug.LogWarning("Cannot modify delay time when it's already playing!");
                return;
            }
            expectedDelay = value;
        }
    }
    public static string LoggingSavePath
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "./Log";
                default:
                    return Path.Combine(Application.persistentDataPath, "Log");
            }
        }
    }
    public const int MaxLoggingCount = 10;
    public static string ConfigurationPath
    {
        get
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "./config/config.json";
                default:
                    return Path.Combine(Application.persistentDataPath, "config/config.json");
            }
        }
    }

    static BuiltInSettings()
    {
        gamePaused = false;
    }
}
//
//MISS | miss | FINE |good| CUTE |good| FINE | tooearly | AWFUL | toofar | UNJUDGED
//

public static class Judging
{
    public static bool IsAwful(float actualStrikeTime, in InPlayingEnvironment.Note note)
    {
        if (actualStrikeTime >= note.StrikeTime)
        {
            return false;
        }
        var delta = note.StrikeTime - actualStrikeTime;
        return delta > BuiltInSettings.tooEarlyLine && delta < BuiltInSettings.farLine;
    }

    public static Score JudgeAccuracy(float actualStrikeTime, in InPlayingEnvironment.Note note, out Latency latency)
    {
        var expectedStrike = note.StrikeTime;

        var delta = Mathf.Abs(actualStrikeTime - expectedStrike);

        if (actualStrikeTime < expectedStrike)
        {
            if (delta > BuiltInSettings.tooEarlyLine && delta < BuiltInSettings.farLine)
            {
                latency = Latency.EARLY;
                return Score.AWFUL;
            }
        }
        else
        {
            if (delta > BuiltInSettings.missLine)
            {
                latency = Latency.LATE;
                return Score.MISS;
            }

            if (delta > BuiltInSettings.goodLine)
            {
                latency = actualStrikeTime > expectedStrike ? Latency.LATE : Latency.EARLY;
                return Score.FINE;
            }
        }
        latency = Latency.OK;
        return Score.CUTE;
    }

    public static ForceScore JudgeForce(int actualForce, int expectedForce)
    {
        if (Math.Abs(actualForce - expectedForce) <= BuiltInSettings.PowerRange)
        {
            return ForceScore.ACCURATE;
        }

        if (actualForce > expectedForce)
        {
            return ForceScore.TOOHEAVY;
        }

        return ForceScore.TOOWEAK;
    }
}