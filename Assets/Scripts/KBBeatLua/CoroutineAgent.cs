using System;
using System.Collections.Generic;
using UnityEngine;
using XLua;

namespace KBBeat.Modding.Lua
{
    public class CoroutineAgent : MonoBehaviour 
    {
        
    }

    public static class CoroutineConfig
    {
        [LuaCallCSharp]
        public static List<Type> LuaCallCSharp
        {
            get
            {
                return new List<Type>()
            {
                typeof(WaitForSeconds),
            };
            }
        }
    }
}