
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace OSM {

    public class OSM_SaveManager {
        public enum SaveState { NoGraph, TempGraph, SavedGraph };

        // The events that dictate the flow of the manager's FSM.
        private enum SaveOp { None, New, Load, Save, SaveAs };
        private SaveOp _saveOp = SaveOp.None;

        private OSM_EditorWindow _window;

        private const string kRootUNEB = "UNEB";

        // Path that stores temporary graphs.
        private const string kTempGraphDirectory = "TempGraphsUNEB";
        private const string kTempFileName = "TempOSM_GraphUNEB";

        public OSM_SaveManager(OSM_EditorWindow w) {
            _window = w;
        }

        private string GetGraphFilePath() {

            string path = EditorUtility.OpenFilePanel("Open Node Graph", "Assets/", "asset");

            // If the path is outside the project's asset folder.
            if (!path.Contains(Application.dataPath)) {

                // If the selection was not cancelled...
                if (!string.IsNullOrEmpty(path)) {
                    _window.ShowNotification(new GUIContent("Please select a Graph asset within the project's Asset folder."));
                    return null;
                }
            }

            return path;
        }

        private void loadGraph(string path) {
            int assetIndex = path.IndexOf("/Assets/");
            path = path.Substring(assetIndex + 1);

            var graph = AssetDatabase.LoadAssetAtPath<OSM_Graph>(path);
            _window.SetGraph(graph);
        }

        private string getSaveFilePath() {
            string path = EditorUtility.SaveFilePanelInProject("Save Node Graph", "NewOSM_Graph", "asset", "Select a destination to save the graph.");

            if (string.IsNullOrEmpty(path)) {
                return "";
            }

            return path;
        }

        #region Save Operations

        public static OSM_Node CreateNode(Type t, OSM_Graph g) {
            try {
                var node = ScriptableObject.CreateInstance(t) as OSM_Node;
                AssetDatabase.AddObjectToAsset(node, g);

                node.Init();
                g.Add(node);
                return node;
            }

            catch (Exception e) {
                throw new UnityException(e.Message);
            }
        }

        public static OSM_Node CreateNode<T>(OSM_Graph g) where T : OSM_Node {
            var node = ScriptableObject.CreateInstance<T>();
            AssetDatabase.AddObjectToAsset(node, g);

            // Optional, set reference to graph: node.graph = g

            node.Init();
            g.Add(node);
            return node;
        }

        public static OSM_Graph CreateOSM_Graph(string path) {

            var graph = ScriptableObject.CreateInstance<OSM_Graph>();

            AssetDatabase.CreateAsset(graph, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return graph;
        }

        private OSM_Graph createNew() {
            string tempPath = getTempFilePath();

            if (!string.IsNullOrEmpty(tempPath)) {

                _window.ShowNotification(new GUIContent("New Graph Created"));
                return CreateOSM_Graph(tempPath);
            }

            return null;
        }

        private bool createNewOnto_Window_WithTempOrEmpty()
        {
            _window.SetGraph(createNew());
            return true;
        }

        private bool createNewOnto_Window_WithSavedgraph()
        {
            // Save the old graph to avoid loss.
            AssetDatabase.SaveAssets();

            _window.SetGraph(createNew());

            return true;
        }

        // Load a graph to a window that has a temp graph active.
        private bool loadOnto_Window_WithTempgraph() {
            string path = GetGraphFilePath();

            if (!string.IsNullOrEmpty(path)) {

                // Get rid of the temporary graph.
                AssetDatabase.DeleteAsset(getCurrentGraphPath());
                loadGraph(path);
                return true;
            }

            return false;
        }

        // Load a graph to a window that has a saved graph active.
        private bool loadOnto_Window_WithSavedgraph() {
            string path = GetGraphFilePath();

            if (!string.IsNullOrEmpty(path)) {

                // Save the old graph.
                save();
                loadGraph(path);
                return true;
            }

            return false;
        }

        // Makes the temporary graph into a saved graph.
        private bool saveTempAs() {
            string newPath = getSaveFilePath();
            string currentPath = getCurrentGraphPath();

            //If asset exists on path, delete it first.
            if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(newPath) != null) {
                AssetDatabase.DeleteAsset(newPath);
            }

            string result = AssetDatabase.ValidateMoveAsset(currentPath, newPath);

            if (result.Length == 0) {
                AssetDatabase.MoveAsset(currentPath, newPath);
                save();
                return true;
            }

            else {
                Debug.LogError(result);
                return false;
            }
        }

        private bool saveCloneAs() {

            string newPath = getSaveFilePath();

            if (!string.IsNullOrEmpty(newPath)) {

                string currentPath = getCurrentGraphPath();

                AssetDatabase.CopyAsset(currentPath, newPath);
                AssetDatabase.SetMainObject(_window.Graph, currentPath);

                save();
                return true;
            }

            return false;
        }

        private bool save() {
            _window.Graph.OnSave();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _window.ShowNotification(new GUIContent("Graph Saved"));
            return true;
        }

        private void saveConnection(NodeConnection conn) {
            if (!AssetDatabase.Contains(conn)) {
                AssetDatabase.AddObjectToAsset(conn, _window.Graph);
            }
        }

        #endregion

        internal void Cleanup() {
        }

        private string getTempFilePath() {
            string tempRoot = getTempDirPath();

            if (string.IsNullOrEmpty(tempRoot)) {
                return "";
            }

            string filename = kTempFileName + _window.GetInstanceID().ToString() + ".asset";
            return tempRoot + "/" + (filename);
        }

        private string getCurrentGraphPath()
        {
            return AssetDatabase.GetAssetPath(_window.Graph);
        }

        private string getTempDirPath() {
            string[] dirs = Directory.GetDirectories(Application.dataPath, kTempGraphDirectory, SearchOption.AllDirectories);

            // Return first occurance containing targetFolderName.
            if (dirs.Length != 0) {
                return getTempPathRelativeToAssets(dirs[0]);
            }

            // Could not find anything. Make the folder
            string rootPath = getPathToRootUNEB();

            if (!string.IsNullOrEmpty(rootPath)) {
                var dirInfo = Directory.CreateDirectory(rootPath + "/" + kTempGraphDirectory);
                return getTempPathRelativeToAssets(dirInfo.FullName);
            }

            else {
                return "";
            }
        }

        private static string getPathToRootUNEB() {
            // Find the UNEB project root directory within the Unity project.
            var dirs = Directory.GetDirectories(Application.dataPath, kRootUNEB, SearchOption.AllDirectories);

            if (dirs.Length != 0) {
                return dirs[0];
            }
            else {
                Debug.LogError("Could not find project root: /" + kRootUNEB + '/');
                return "";
            }
        }

        private static string getTempPathRelativeToAssets(string fullTempPath)
        {
            int index = fullTempPath.IndexOf("Assets");
            return fullTempPath.Substring(index);
        }
    }
}