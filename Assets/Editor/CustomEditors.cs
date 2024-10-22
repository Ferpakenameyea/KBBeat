﻿using KBbeat;
using KBbeat.Debugger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tester))]
public class TesterInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var tester = target as Tester;
    }
}

public class EditorTools : Editor
{
    [MenuItem("Tools/GenerateAssetBundleForWindows")]
    private static void GenerateWindows()
    {
        string path = Application.streamingAssetsPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        AssetDatabase.Refresh();
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        UnityEngine.Debug.Log("Build asset bundles succeeded");
    }

    [MenuItem("Tools/GenerateAssetBundleForAndroid")]
    private static void GenerateAndroid()
    {
        string path = Application.streamingAssetsPath;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        AssetDatabase.Refresh();
        BuildPipeline.BuildAssetBundles(path, BuildAssetBundleOptions.None, BuildTarget.Android);
        UnityEngine.Debug.Log("Build asset bundles succeeded");
    }

    [MenuItem("Tools/GenerateWithoutLvl1ForWindows")]
    public static void GenerateNolvl1_Windows()
    {
        string path = Application.streamingAssetsPath;
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            var dir = new DirectoryInfo(path);    
            foreach (var file in dir.GetFiles())
            {
                Debug.LogFormat("Deleting item: {0}", file);
                file.Delete();
            }
        }

        var names = AssetDatabase.GetAllAssetBundleNames().Where((name) => !name.Contains("level1"));

        List<AssetBundleBuild> builds = new();

        foreach (var bundlename in names)
        {
            Debug.LogFormat("Building: {0}", bundlename);
            builds.Add(new()
            {
                assetBundleName = bundlename,
                assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundlename)
            });
        }

        BuildPipeline.BuildAssetBundles(path, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

        Debug.Log("DONE!");
    }

    [MenuItem("Tools/GenerateOnlyLvl1ForWindows")]
    public static void GenerateOnlyLvl1_Windows()
    {
        string path = Application.streamingAssetsPath;
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        else
        {
            var dir = new DirectoryInfo(path);    
            foreach (var file in dir.GetFiles())
            {
                Debug.LogFormat("Deleting item: {0}", file);
                file.Delete();
            }
        }

        var names = AssetDatabase.GetAllAssetBundleNames().Where((name) => name.Contains("level1"));

        List<AssetBundleBuild> builds = new();

        foreach (var bundlename in names)
        {
            Debug.LogFormat("Building: {0}", bundlename);
            builds.Add(new()
            {
                assetBundleName = bundlename,
                assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundlename)
            });
        }

        BuildPipeline.BuildAssetBundles(path, builds.ToArray(), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

        Debug.Log("DONE!");
    }
}

[CustomEditor(typeof(LevelSelector))]
public class SelectorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var component = target as LevelSelector;
        if (GUILayout.Button("SwitchLeft"))
        {
            component.SwitchLeft();
        }
        if (GUILayout.Button("SwitchRight"))
        {
            component.SwitchRight();
        }
    }
}

[CustomEditor(typeof(GameEndUITester))]
public class GameEndUITesterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        var component = target as GameEndUITester;
        if (GUILayout.Button("TestShow"))
        {
            component.TestShowRank();
        }
        if (GUILayout.Button("TestAcc"))
        {
            component.TestShowAcc();
        }
        if (GUILayout.Button("TestScore"))
        {
            component.TestShowScore();
        }
    }
}

public class SmoothNormalTool
{
    [MenuItem("Tools/平滑法线、写入切线数据")]
    public static void WriteAverageNormalToTangentToos()
    {
        var meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
        var mesh = meshFilter.sharedMesh;
        WirteAverageNormalToTangent(mesh);
    }
    private static void WirteAverageNormalToTangent(Mesh mesh)
    {
        var averageNormalHash = new Dictionary<Vector3, Vector3>();
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            if (!averageNormalHash.ContainsKey(mesh.vertices[j]))
            {
                averageNormalHash.Add(mesh.vertices[j], mesh.normals[j]);
            }
            else
            {
                averageNormalHash[mesh.vertices[j]] =
                    (averageNormalHash[mesh.vertices[j]] + mesh.normals[j]).normalized;
            }
        }
        var averageNormals = new Vector3[mesh.vertexCount];
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            averageNormals[j] = averageNormalHash[mesh.vertices[j]];
        }

        var tangents = new Vector4[mesh.vertexCount];
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            tangents[j] = new Vector4(averageNormals[j].x, averageNormals[j].y, averageNormals[j].z, 0);
        }
        mesh.tangents = tangents;
    }
}

[CustomEditor(typeof(InGameUI))]
public class InGameUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("TestLate"))
        {
            typeof(InGameUI).GetMethod("LateWarn", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)?.Invoke(target, Array.Empty<object>());            
        }

        if (GUILayout.Button("TestHeavy"))
        {
            typeof(InGameUI).GetMethod("HeavyWarn",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance)?.Invoke(target, Array.Empty<object>());
        }
    }
}