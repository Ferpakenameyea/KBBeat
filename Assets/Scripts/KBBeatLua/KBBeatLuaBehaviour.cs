using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;


namespace KBBeat.Modding.Lua
{
    public class KBBeatLuaBehaviour : MonoBehaviour
    {

    #region Global
        private static LuaEnv luaEnv = new();
        private static float lastGC = 0;
        internal const float GCInterval = 1;
    #endregion

    #region Lua
        [SerializeField] private TextAsset luaScriptAsset;
        private string luaScript;
        public LuaTable ScopeTable { get; private set; }
    #endregion

    #region LuaMono
        private Action luaAwake;
        private Action luaStart;
        private Action luaOnEnable;
        private Action luaOnDisable;
        private Action luaUpdate;
        private Action luaLateUpdate;
        private Action luaFixedUpdate;
        private Action luaOnDestroy;
        private Dictionary<string, LuaEventHandler> exportedFunctions = new();
        public delegate void LuaEventHandler(object arg);
    #endregion

    #region Injection
        [Header("Initialization Injections")]
        [SerializeField] private LuaInjection<int>[] ints;
        [SerializeField] private LuaInjection<float>[] floats;
        [SerializeField] private LuaInjection<double>[] doubles;
        [SerializeField] private LuaInjection<bool>[] bools;
        [SerializeField] private LuaInjection<string>[] strings;
        [SerializeField] private LuaInjection<Vector3>[] vectors;
        [SerializeField] private LuaInjection<Color>[] colors;
        [SerializeField] private LuaInjection<UnityEngine.Object>[] objects;
    #endregion

        private object currentEventArg;

        protected object[] InitializationReturns { get; private set; }

        static KBBeatLuaBehaviour()
        {
            luaEnv.Global.Set<string, Action<string>>("UnityLog", Debug.Log);
        }

        protected virtual void Awake() 
        {
            this.luaScript = luaScriptAsset.text;

            this.ScopeTable = luaEnv.NewTable();
            using (LuaTable meta = luaEnv.NewTable())
            {
                meta.Set("__index", luaEnv.Global);
                ScopeTable.SetMetaTable(meta);
            }

            ScopeTable.Set("this", this);

            InitializationReturns = luaEnv.DoString(luaScript, luaScriptAsset.name, ScopeTable);

            ScopeTable.Get("Awake", out luaAwake);
            ScopeTable.Get("Start", out luaStart);
            ScopeTable.Get("OnEnable", out luaOnEnable);
            ScopeTable.Get("OnDisable", out luaOnDisable);
            ScopeTable.Get("Update", out luaUpdate);
            ScopeTable.Get("LateUpdate", out luaLateUpdate);
            ScopeTable.Get("FixedUpdate", out luaFixedUpdate);
            ScopeTable.Get("OnDestroy", out luaOnDestroy);
            
            Inject();

            RegisterEvents();

            luaAwake?.Invoke();
            
        } 

        private void RegisterEvents()
        {
            if (InitializationReturns == null)
            {
                return;
            }

            if (InitializationReturns[0] is not LuaTable)
            {
                Debug.LogWarningFormat($"Lua script of {0} didn't return a table, returns will be ignored.");
                return;
            }

            var table = InitializationReturns[0] as LuaTable;
            var events = table.Get<LuaTable>("events");
        
            if (events == null)
            {
                return;
            }

            try
            {
                int counter = 0;
                events.ForEach((string name, LuaEventHandler handler) => {
                    exportedFunctions.Add(name, handler);
                    counter++;
                });

                Debug.LogFormat("Loaded {0} events from lua {1}", counter, this.luaScriptAsset.name);
            }
            catch (Exception err)
            {
                Debug.LogErrorFormat("Exception when registering lua events for script {0}", this.luaScriptAsset.name);
                Debug.LogException(err);
            }
        }

#region UnityEvents Interface
        public void PassIntArg(int arg)
        {
            this.currentEventArg = arg;
        }

        public void PassFloatArg(float arg)
        {
            this.currentEventArg = arg;
        }

        public void PassStringArg(string arg)
        {
            this.currentEventArg = arg;
        }

        public void PassObjectArg(UnityEngine.Object arg)
        {
            this.currentEventArg = arg;
        }
#endregion

        public void InvokeEvent(string name)
        {
            if (this.exportedFunctions.TryGetValue(name, out var func))
            {
                try
                {
                    func.Invoke(currentEventArg);
                    currentEventArg = null;
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Lua error in script {0}", this.luaScriptAsset.name);
                    Debug.LogException(e);
                    return;
                }
            }
            else
            {
                Debug.LogErrorFormat("Event handler {0} not found in lua script {1}", name, this.luaScriptAsset.name);
            }
        }


        protected virtual void Start() 
        {
            luaStart?.Invoke();
        }

        protected virtual void Update() 
        {
            luaUpdate?.Invoke();

            if (Time.time - lastGC > GCInterval)
            {
                luaEnv.Tick();
                lastGC = Time.time;
            }
        }

        protected virtual void FixedUpdate() 
        {
            luaFixedUpdate?.Invoke();
        }

        protected virtual void LateUpdate() 
        {
            luaLateUpdate?.Invoke();
        }

        protected virtual void OnEnable() 
        {
            luaOnEnable?.Invoke();       
        }

        protected virtual void OnDisable() 
        {
            luaOnDisable?.Invoke();
        }

        protected virtual void OnDestroy() 
        {
            luaOnDestroy?.Invoke();

            ScopeTable?.Dispose();

            luaAwake = null;
            luaStart = null;
            luaUpdate = null;
            luaFixedUpdate = null;
            luaOnEnable = null;
            luaOnDisable = null;
            luaOnDestroy = null;
            luaOnDisable  = null;
        }

        private void Inject()
        {
            // 注入 int 类型变量
            foreach (var i in ints)
            {
                ScopeTable.Set(i.fieldName, i.value);
            }

            // 注入 float 类型变量
            foreach (var f in floats)
            {
                ScopeTable.Set(f.fieldName, f.value);
            }

            // 注入 double 类型变量
            foreach (var d in doubles)
            {
                ScopeTable.Set(d.fieldName, d.value);
            }

            // 注入 bool 类型变量
            foreach (var b in bools)
            {
                ScopeTable.Set(b.fieldName, b.value);
            }

            // 注入 Vector3 类型变量
            foreach (var v in vectors)
            {
                ScopeTable.Set(v.fieldName, v.value);
            }

            // 注入 Color 类型变量
            foreach (var c in colors)
            {
                ScopeTable.Set(c.fieldName, c.value);
            }

            // 注入 GameObject 类型变量
            foreach (var ob in objects)
            {
                ScopeTable.Set(ob.fieldName, ob.value);
            }

            // 注入 string 类型变量
            foreach (var str in strings)
            {
                ScopeTable.Set(str.fieldName, str.value);
            }
        }

        public void ResetInitializationVariables()
        {
            this.Inject();
        }
    }

    [Serializable]
    public class LuaInjection<T>
    {
        public string fieldName;
        public T value;
    }

    [Serializable]
    public struct ArgPair<T>
    {
        public string eventName;
        public T value;
    }
}
