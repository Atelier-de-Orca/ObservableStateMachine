using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using NodeEditorFramework.Utilities;

namespace OSM {

    public class OSM_EditorState {

        public OSM_Node selectedNode;
        public Vector2 lastClickedPosition;

        public NodeOutput selectedOutput;
        public NodeInput selectedInput;

        public System.Type typeToCreate;
    }

    public class OSM_EditorWindow : EditorWindow {

        [MenuItem("Window/OSM Editor")]
        static void Init() {
            var w = EditorWindow.CreateInstance<OSM_EditorWindow>();
            w.titleContent = new GUIContent("OSM Editor");
            w.Show();
        }

        public float ToolbarHeight => 20f;
        public float ToolbarButtonWidth => 50f;

        [SerializeField]
        public OSM_Graph _graph;
        public OSM_Graph Graph {
            get {
                if (_graph == null)
                    _graph = new OSM_Graph<object>();

                return _graph;
            }
            set {
                _graph = value;
            }
        }
        
        public OSM_Editor editor;
        public OSM_EditorState state;
        
        public enum Mode { Edit, View };
        private Mode _mode = Mode.Edit;

        void OnEnable() {

            GUIScaleUtility.CheckInit();

            editor = new OSM_Editor(this);
            state = new OSM_EditorState();

            editor.graph = Graph;
            _mode = Mode.Edit;

            editor.HomeView();
        }

        void OnDisable() { }

        void OnDestroy() { }

        void OnGUI() {

            editor.Draw();

            OnEventProcess(Event.current);
        }

        private void OnEventProcess(Event e) {
            if (e.isMouse && e.button == 0) {
                if (e.type == EventType.MouseDown) {
                    // Debug.Log("Left Mouse Down");
                }
                else if (e.type == EventType.MouseUp) {
                    // Debug.Log("Left Mouse Up");
                }
            }
            else if (e.isMouse && e.button == 1) {
                if (e.type == EventType.MouseDown) {
                    // Debug.Log("Right Mouse Down");
                }
                else if (e.type == EventType.MouseUp) {
                    // Debug.Log("Right Mouse Up");
                    GenericMenu genericMenu = new GenericMenu();
                    genericMenu.AddItem(new GUIContent("Add Node"), false,
                    () => {
                        Graph.CreateNode(e.mousePosition);
                    });
                    genericMenu.ShowAsContext();
                }
            }
        }

        public void SetGraph(OSM_Graph g, Mode mode = Mode.Edit) {
            Graph = g;
            editor.graph = g;

            _mode = mode;
        }

        public Rect Size { get { return new Rect(Vector2.zero, position.size); } }
        public Rect InputRect {
            get {
                var rect = Size;

                rect.y += ToolbarHeight;
                rect.height -= ToolbarHeight;

                return rect;
            }
        }

        public Mode GetMode() { return _mode; }


        [OnOpenAsset(1)]
        private static bool OpenGraphAsset(int instanceID, int line) {
            var graphSelected = EditorUtility.InstanceIDToObject(instanceID) as OSM_Graph;

            if (graphSelected != null) {

                OSM_EditorWindow windowToUse = null;

                // Try to find an editor window without a graph...
                var windows = Resources.FindObjectsOfTypeAll<OSM_EditorWindow>();
                foreach (var w in windows) {

                    // The canvas is already opened
                    if (w.Graph == graphSelected) {
                        return false;
                    }

                    // Found a window with no active canvas.
                    if (w.Graph == null) {
                        windowToUse = w;
                        break;
                    }
                }

                // No windows available...just make a new one.
                if (!windowToUse) {
                    windowToUse = EditorWindow.CreateInstance<OSM_EditorWindow>();
                    windowToUse.titleContent = new GUIContent("Node Editor");
                    windowToUse.Show();
                }

                windowToUse.SetGraph(graphSelected);
                // windowToUse._saveManager.InitState();
                windowToUse.Repaint();

                return true;
            }

            return false;
        }
    }
}