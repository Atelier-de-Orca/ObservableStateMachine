
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace OSM {

    public class OSM_SaveManager {

        private OSM_EditorWindow _window;

        private const string kRootUNEB = "UNEB";

        // Path that stores temporary graphs.
        private const string kTempGraphDirectory = "TempGraphsUNEB";
        private const string kTempFileName = "TempOSM_GraphUNEB";

        public OSM_SaveManager(OSM_EditorWindow w) {
            _window = w;
        }

        public void CreateNewGraph(Type type, string path) {

            int assetIndex = path.IndexOf("/Assets");
            path = path.Substring(assetIndex + 1);

            var graph = ScriptableObject.CreateInstance(type.ToString());

            Debug.Log(path);

            AssetDatabase.CreateAsset(graph, path + $"/{type.ToString()}(Default).asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public OSM_Node CreateNewNode(Type type) {

            try {
                Debug.Log(type);
                var node = ScriptableObject.CreateInstance(type);
                AssetDatabase.AddObjectToAsset(node, _window.Graph);
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(node));

                ((OSM_Node)node).Init();
                return (OSM_Node)node;
            }
            catch (Exception e) {
                throw new UnityException(e.Message);
            }
        }

        public void SaveNodeInGraph() {

        }

        public void OpenGraph() {
            LoadGraph(GetGraphFilePath());
        }

        public void LoadGraph(string path) {
            if(string.IsNullOrEmpty(path)) return;

            int assetIndex = path.IndexOf("/Assets/");
            path = path.Substring(assetIndex + 1);

            var graph = AssetDatabase.LoadAssetAtPath<OSM_Graph>(path);
            _window.SetGraph(graph);
        }

        private string GetGraphFilePath() {

            string path = EditorUtility.OpenFilePanel("Open Node Graph", "Assets/", "asset");

            if (!path.Contains(Application.dataPath)) {

                if (!string.IsNullOrEmpty(path)) {
                    _window.ShowNotification(new GUIContent("Please select a Graph asset within the project's Asset folder."));
                    return string.Empty;
                }
            }

            return path;
        }

        private string getSaveFilePath() {
            string path = EditorUtility.SaveFilePanelInProject("Save Node Graph", "NewOSM_Graph", "asset", "Select a destination to save the graph.");

            if (string.IsNullOrEmpty(path)) {
                return "";
            }

            return path;
        }

        private bool SaveAll() {
            _window.Graph.OnSave();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _window.ShowNotification(new GUIContent("Graph Saved"));
            return true;
        }

        private void SaveConnection(NodeConnection conn) {
            if (!AssetDatabase.Contains(conn)) {
                AssetDatabase.AddObjectToAsset(conn, _window.Graph);
            }
        }

        internal void Cleanup() {
        }
    }
}