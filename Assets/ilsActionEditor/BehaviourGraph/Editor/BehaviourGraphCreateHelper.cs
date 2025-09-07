using System;
using System.IO;
using System.Reflection;
using ilsFramework.Core;
using UnityEditor;
using UnityEngine;

namespace ilsActionEditor.Editor
{
    public static class BehaviourGraphCreateHelper
    {
        public static BaseBehaviourGraph CreateGraph(Type graphType)
        {
            BaseBehaviourGraph baseGraph = ScriptableObject.CreateInstance(graphType) as BaseBehaviourGraph;
            //傻逼Unity ，经典private
            MethodInfo getActiveFolderPath = typeof(ProjectWindowUtil).GetMethod(
                "GetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic);

            string folderPath = (string) getActiveFolderPath.Invoke(null, null);
            
            string panelFileName = "Graph";
            string path = EditorUtility.SaveFilePanelInProject("Save Graph Asset", panelFileName, "asset", "", folderPath);
            if (string.IsNullOrEmpty(path))
            {
                Debug.Log("创建graph已取消");
                return null;
            }
            AssetDatabase.CreateAsset(baseGraph, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return baseGraph;
        }
    }
}