using System.Diagnostics;
using System.IO;
using ilsFramework.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = System.Diagnostics.Debug;

namespace ilsActionEditor
{
    public class EditorResourceLoader
    {
        private static EditorResourceLoader _instance;
        
        public static EditorResourceLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    
                    _instance = new EditorResourceLoader();
                    _instance.LoadAllResources();
                }
                return _instance;
            }
        }

        public string ResourceRoot { get;private set; }
        public string BackGroundStylePath =>ResourceRoot +  "Styles/BackGroundStyle.uss";
        public StyleSheet BackGroundStyle { get;private set; }
        
        public string ContentContainerStylePath =>ResourceRoot +  "Styles/InspectorStyle.uss";
        public StyleSheet ContentContainerStyle { get;private set; }

        public string CreateNewIconPath => ResourceRoot + "Resource/CreateNew.png";
        public Texture2D CreateNewIcon { get;private set; }
        
        public string BlackBoardIconPath => ResourceRoot + "Resource/Blackboard.png";
        public Texture2D BlackBoardIcon { get;private set; }
        
        public string ToggleMipMapIconPath => ResourceRoot + "Resource/MiniMap.png";
        public Texture2D ToggleMipMapIcon { get;private set; }
        
        public string PingToAssetIconPath => ResourceRoot + "Resource/GotoFile.png";
        public Texture2D PingToAssetIcon { get;private set; }
        
        public void LoadAllResources()
        {
            var st = new StackTrace(1, true);
            var rawScriptPath = st.GetFrame(0).GetFileName();
            var rootPath =ConvertToUnityRelativePath(new DirectoryInfo(rawScriptPath).Parent.Parent.FullName) + "/" ;
            ResourceRoot = rootPath;
            
            BackGroundStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(BackGroundStylePath);
            ContentContainerStyle = AssetDatabase.LoadAssetAtPath<StyleSheet>(ContentContainerStylePath);
            CreateNewIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(CreateNewIconPath);
            BlackBoardIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(BlackBoardIconPath);
            ToggleMipMapIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(ToggleMipMapIconPath);
            PingToAssetIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(PingToAssetIconPath);
            
        }
        
        /// <summary>
        /// 将全局路径转换为Unity项目相对路径
        /// </summary>
        /// <param name="fullPath">完整的文件系统路径</param>
        /// <returns>Unity相对路径 (如 "Assets/Textures/image.png")</returns>
        public static string ConvertToUnityRelativePath(string fullPath)
        {
            // 获取项目根目录路径
            string projectRoot = Path.GetFullPath(Application.dataPath + "/..");
        
            // 确保路径格式统一
            fullPath = Path.GetFullPath(fullPath).Replace('\\', '/');
            projectRoot = projectRoot.Replace('\\', '/');
        
            // 检查路径是否在项目内
            if (!fullPath.StartsWith(projectRoot))
            {
                UnityEngine.Debug.LogWarning($"路径不在Unity项目内: {fullPath}");
                return null;
            }
        
            // 转换为相对路径
            string relativePath = fullPath.Substring(projectRoot.Length);
        
            // 规范化路径格式
            relativePath = relativePath.TrimStart('/');
        
            // 特殊处理：Assets文件夹需要显式标记
            if (relativePath.StartsWith("Assets/"))
            {
                return relativePath;
            }
        
            // 处理路径在Assets目录内但未标记的情况
            if (relativePath.StartsWith("Assets"))
            {
                return "Assets/" + relativePath.Substring(6);
            }
        
            return "Assets/" + relativePath;
        }
    }
}