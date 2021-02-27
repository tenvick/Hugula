﻿// Copyright (c) 2022 hugula
// direct https://github.com/tenvick/hugula
//
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Hugula;
using Hugula.Databinding;
using Hugula.Framework;
using Hugula.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Hugula {

    [XLua.LuaCallCSharp]
    public class EnterLua : MonoBehaviour, IManager {

#if UNITY_EDITOR
        const string KeyDebugString = "_Plua_Debug_string";

        public static bool isDebug {
            get {
                bool _debug = EditorPrefs.GetBool (KeyDebugString, true);
                return _debug;
            }
            set {
                EditorPrefs.SetBool (KeyDebugString, value);
            }
        }
#endif

        [SerializeField] string enterLua = "begin"; //main

        internal static LuaEnv luaenv;

        // Start is called before the first frame update
        void Awake () {
            DontDestroyOnLoad (this.gameObject);
            var ins = Executor.instance;
            ins = null;
            ReloadLua ();
            Manager.Add (this.GetType (), this);
        }

        IEnumerator Start () {
            yield return ResLoader.Ready;
            luaenv.DoString ("require('" + enterLua + "')");
#if UNITY_EDITOR
            Debug.LogFormat ("<color=green>running {0} mode </color> <color=#8cacbc> change( menu Hugula->Debug Lua)</color>", isDebug ? "debug" : "release");
#endif
        }

        // Update is called once per frame
        void Update () {
            if (luaenv != null) {
                luaenv.Tick ();
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetKeyUp (KeyCode.F5)) {
                GlobalDispatcher.instance.Dispatch (DispatcherEvent.F5, null);
            }

#endif
        }

        void OnDestroy () {
            Manager.Remove (this.GetType ());

            luaenv?.DoString (@"
                local util = require 'xlua.util'
        util.print_func_ref_by_csharp()");
            // if (luaenv != null) luaenv.Dispose ();
        }

        void OnApplicationQuit () {
            Debug.Log ("OnApplicationQuit");
        }

        private void BeforeLuaDispose () {
            // Manager.Terminate();
        }

        private void ClearLuaRef () {
            BeforeLuaDispose ();
        }

        private void ReloadLua () {
            // LuaEntry
            if (luaenv != null) {
                ClearLuaRef ();
                luaenv.Dispose ();
                luaenv = null;
            }
            luaenv = new LuaEnv ();
            luaenv.AddLoader (Loader);
        }

        /// <summary>
        ///  loader
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private byte[] Loader (ref string name) {
            byte[] str = null;

#if UNITY_EDITOR && !USE_CDN
            string path = Application.dataPath + "/Lua/" + name.Replace ('.', '/') + ".lua";
            bool exist = File.Exists (path);
            if (isDebug && exist) {
                str = File.ReadAllBytes (path); //LuaState.CleanUTF8Bom(
            } else {
                if (!exist) Debug.LogErrorFormat ("file {0} does't exists. ", path);
                str = LoadLuaBytes (Common.LUA_PREFIX + name.Replace ('.', '+'));
            }
#elif UNITY_STANDALONE && !HUGULA_RELEASE

            var path = Application.dataPath + "/config_data/" + name.Replace ('.', '/') + ".lua"; //���ȶ�ȡ�����ļ�
            if (File.Exists (path)) {
                str = File.ReadAllBytes (path);
            } else {
                str = LoadLuaBytes (Common.LUA_PREFIX + name.Replace ('.', '+'));
            }
            if (str == null) Debug.LogErrorFormat ("lua({0}) path={1} not exists.", name, path);
#else
#if !HUGULA_RELEASE||LUA_ANDROID_DEBUG
            var path = Application.persistentDataPath + "/lua/" + name.Replace ('.', '/') + ".lua";
            if (File.Exists (path)) {
                str = File.ReadAllBytes (path);
            } else
#endif  
            {
                str = LoadLuaBytes (Common.LUA_PREFIX + name.Replace ('.', '+'));
            }
#endif

#if UNITY_EDITOR
            if (str == null) {
                if (isDebug)
                    Debug.LogErrorFormat ("lua ({0}) path={1} not exists.", name, path);
                else
                    Debug.LogErrorFormat ("the file(Assets/LuaBytes/lua_bundle/{0}.lua)  did't exists.", name);

            }

            name = path;//chunkname 调试用
#endif
            return str;
        }

        /// <summary>
        /// load lua bytes
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private byte[] LoadLuaBytes (string name) {
            byte[] ret = null;
            var txt = ResLoader.LoadAsset<TextAsset> (name);
            if (txt != null) {
                ret = txt.bytes;
                // ResLoader.Release(txt);
            }

            return ret;

        }

        public void Initialize () {
            // luaenv.DoString ("require('" + enterLua + "')");
        }
        public void Terminate () {
            GlobalDispatcher.instance.Dispose ();
            ExpressionUtility.instance.Dispose ();
        }

        //重启动游戏
        public static void ReOpen (float sconds) {
            UnityEngine.SceneManagement.SceneManager.LoadScene (0);
            Debug.LogFormat ("ReOpen !");
        }

        #region delay

        public static void StopDelay (object arg) {
            var ins = Manager.Get<EnterLua> ();
            if (ins != null && arg is IEnumerator)
                ins.StopCoroutine ((IEnumerator) arg);
        }

        public static IEnumerator Delay (LuaFunction luafun, float time, object args) {
            var ins = Manager.Get<EnterLua> ();
            var _corout = DelayDo (luafun, time, args);
            ins.StartCoroutine (_corout);
            return _corout;
        }

        private static IEnumerator DelayDo (LuaFunction luafun, float time, object args) {
            yield return new WaitForSeconds (time);
            luafun.Call (args);
        }

        public static IEnumerator DelayFrame (LuaFunction luafun, int frame, object args) {
            var ins = Manager.Get<EnterLua> ();
            var _corout = DelayFrameDo (luafun, frame, args);
            ins.StartCoroutine (_corout);
            return _corout;
        }

        private static IEnumerator DelayFrameDo (LuaFunction luafun, int frame, object args) {
            var waitFrame = WaitForFrameCountPool.Get ();
            waitFrame.SetEndFrame (frame);
            yield return waitFrame;
            WaitForFrameCountPool.Release (waitFrame);
            luafun.Call (args);
        }

        static Hugula.Utils.ObjectPool<WaitForFrameCount> WaitForFrameCountPool = new Hugula.Utils.ObjectPool<WaitForFrameCount> (null, null);

        [XLua.LuaCallCSharp]
        public class WaitForFrameCount : IEnumerator {
            int m_EndCount;
            public WaitForFrameCount (int frameCount) {
                SetEndFrame (frameCount);
            }

            public WaitForFrameCount () {

            }

            public void SetEndFrame (int frameCount) {
                m_EndCount = Time.frameCount + frameCount;
            }

            bool IEnumerator.MoveNext () {
                return Time.frameCount <= m_EndCount;
            }

            object IEnumerator.Current {
                get {
                    return null;
                }
            }

            void IEnumerator.Reset () {

            }
        }
        #endregion
    }
}
