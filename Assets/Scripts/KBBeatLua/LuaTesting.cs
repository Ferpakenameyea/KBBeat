using System;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using XLua;

public class LuaTesting : MonoBehaviour
{
    [SerializeField] private TextAsset luaScript;

    private LuaEnv luaEnv;

    private void Start() 
    {
        luaEnv = new LuaEnv();
        luaEnv.DoString(luaScript.text);
        var function = luaEnv.Global.Get<Func<LuaObject1>>("GetObject");
        Assert.IsNotNull(function);

        var functionProvidedObject = function.Invoke();

        Debug.LogFormat("MyAttribute is {0}", functionProvidedObject.MyAttribute);
    
        var luaTable = luaEnv.Global.Get<LuaTable1>("GlobalTable");
        Assert.IsNotNull(luaTable);
        Debug.LogFormat("LuaTable name is {0}", luaTable.tableName);
    }

    private interface LuaObject1
    {
        public int MyAttribute { get; }
    }

    private class LuaTable1
    {
        public string tableName;
    }
}