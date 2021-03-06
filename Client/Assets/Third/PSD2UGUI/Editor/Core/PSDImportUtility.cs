﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace PSDUINewImporter
{
    public static class PSDImportUtility
    {
        public static string baseFilename;
        public static string baseDirectory;
        public static Canvas canvas;
        public static GameObject eventSys;
        // public static readonly Dictionary<Transform, Transform> ParentDic = new Dictionary<Transform, Transform>();
        //需要新建gameobject
        public const string NewTag = "New";
        //布局组件
        public const string SizeTag = "Size";
        //不需要创建
        public const string HideTag = "Hide";
        //需要绑定控件
        public const string DynamicTag = "Dynamic";

        public static bool NeedDraw(Layer layer)
        {
            return (!layer.TagContains(HideTag) || layer.TagContains(DynamicTag)) && layer.target == null;
        }

        public static bool ChildrenLayersTagContains(Layer layer, string tag, out int index)
        {
            Layer lItem;
            for (var i = 0; i < layer.layers.Length; i++)
            {
                lItem = layer.layers[i];
                if (lItem.TagContains(tag))
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        public static object DeserializeXml(string filePath, System.Type type)
        {
            object instance = null;
            StreamReader xmlFile = File.OpenText(filePath);
            if (xmlFile != null)
            {
                string xml = xmlFile.ReadToEnd();
                if ((xml != null) && (xml.ToString() != ""))
                {
                    XmlSerializer xs = new XmlSerializer(type);
                    System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
                    byte[] byteArray = encoding.GetBytes(xml);
                    MemoryStream memoryStream = new MemoryStream(byteArray);
                    XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, System.Text.Encoding.UTF8);
                    if (xmlTextWriter != null)
                    {
                        instance = xs.Deserialize(memoryStream);
                    }
                }
            }
            xmlFile.Close();
            return instance;
        }

        //         public static T InstantiateItem<T>(string resourcePatn, string name,GameObject parent) where T : UnityEngine.Object
        //         {
        //             GameObject temp = Resources.Load(resourcePatn, typeof(GameObject)) as GameObject;
        //             GameObject item = GameObject.Instantiate(temp) as GameObject;
        //             item.name = name;
        //             item.transform.SetParent(canvas.transform, false);
        //             ParentDic[item.transform] =  parent.transform;
        //             return item.GetComponent<T>();
        //         }

        /// <summary>
        /// 加载并实例化prefab，编辑器下不用Resources.Load，否则这些预设会打到安装包
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetPath">assets全路径，带后缀</param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T LoadAndInstant<T>(string assetPath, string name, GameObject parent) where T : UnityEngine.Object
        {
            GameObject temp = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
            Debug.LogFormat(" LoadAndInstant assetPath={0}",assetPath);
            GameObject item = GameObject.Instantiate(temp, Vector3.zero, Quaternion.identity, parent?.transform) as GameObject;
            if (item == null)
            {
                Debug.LogError("LoadAndInstant asset failed : " + assetPath);
                return null;
            }
            item.name = name;
            //item.transform.SetParent(parent.transform);
            // item.transform.SetParent(canvas.transform, false);
            // ParentDic[item.transform] = parent.transform;
            item.transform.localScale = Vector3.one;
            return item.GetComponent<T>();
        }

        public static void SetAnchorMiddleCenter(this RectTransform rectTransform)
        {
            if (rectTransform == null)
            {
                Debug.LogWarning("rectTransform is null...");
                return;
            }
            rectTransform.offsetMin = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMax = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        }
        /**
        public static Object LoadAssetAtPath<T>(this PSImage image)
        {
            string assetPath = "";
            if (image.imageSource == ImageSource.Common || image.imageSource == ImageSource.Custom)
            {
                assetPath = PSDImportUtility.baseDirectory + image.name + PSDImporterConst.PNG_SUFFIX;
            }
            else
            {
                assetPath = PSDImporterConst.Globle_BASE_FOLDER + image.name.Replace(".", "/") + PSDImporterConst.PNG_SUFFIX;
            }

            Object obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(T));
            if (obj == null)
            {
                Debug.LogWarning("loading asset is null, at path: " + assetPath);
            }

            return obj;
        }
        **/

        public static string FindFileInDirectory(string baseDirectory,string fileName)
        {
            System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(baseDirectory);
            var files = dirInfo.GetFiles(fileName,System.IO.SearchOption.AllDirectories);
            if (files.Length > 0) return files[0].FullName.Replace("\\","/").Replace(Application.dataPath, "Assets");
            return string.Empty;
        }
        
        public static RectTransform GetRectTransform(this GameObject source)
        {
            return source.GetComponent<RectTransform>();
        }

        public static T AddMissingComponent<T>(this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if (comp == null)
                comp = go.AddComponent<T>();
            return comp;
        }

        public static void DestroyComponent<T>(this GameObject go) where T : Component
        {
            if (go == null)
                return;

            T comp = go.GetComponent<T>();
            if (comp != null)
                UnityEngine.Object.Destroy(comp);
        }
    }
}