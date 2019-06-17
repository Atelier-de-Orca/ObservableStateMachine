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

        public const float kToolbarHeight = 20f;
        public const float kToolbarButtonWidth = 50f;

        [SerializeField]
        public OSM_Graph graph;
        
        public OSM_Editor editor;
        public OSM_EditorState state;
        
        public enum Mode { Edit, View };
        private Mode _mode = Mode.Edit;

        void OnEnable() {

            GUIScaleUtility.CheckInit();

            editor = new OSM_Editor(this);
            state = new OSM_EditorState();

            editor.graph = graph;
            _mode = Mode.Edit;

            editor.HomeView();
        }

        void OnDisable() { }

        void OnDestroy() { }

        void OnGUI() {

            editor.Draw();
            drawToolbar();
        }

        public void SetGraph(OSM_Graph g, Mode mode = Mode.Edit) {
            graph = g;
            editor.graph = g;

            _mode = mode;
        }

        private void drawToolbar() {
            EditorGUILayout.BeginHorizontal("Toolbar");

            if (DropdownButton("File", kToolbarButtonWidth)) {
                CreateFileMenu();
            }

            if (DropdownButton("Edit", kToolbarButtonWidth)) {
                CreateEditMenu();
            }

            if (DropdownButton("View", kToolbarButtonWidth)) {
                CreateViewMenu();
            }

            if (DropdownButton("Settings", kToolbarButtonWidth + 10f)) {
                CreateSettingsMenu();
            }

            if (DropdownButton("Tools", kToolbarButtonWidth)) {
                CreateToolsMenu();
            }

            GUILayout.FlexibleSpace();
            DrawGraphName();
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawGraphName() {

            string graphName = "None";
            if (graph != null) {
                graphName = graph.GetName();
            }

            GUILayout.Label(graphName);
        }

        private void CreateFileMenu() {
            
            var menu = new GenericMenu();
            menu.DropDown(new Rect(5f, kToolbarHeight, 0f, 0f));
        }

        private void CreateEditMenu() {

            var menu = new GenericMenu();
            menu.DropDown(new Rect(55f, kToolbarHeight, 0f, 0f));
        }

        private void CreateViewMenu() {

            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Home"), false, editor.HomeView);
            menu.AddItem(new GUIContent("Zoom In"), false, () => { editor.Zoom(-1); });
            menu.AddItem(new GUIContent("Zoom Out"), false, () => { editor.Zoom(1); });

            menu.DropDown(new Rect(105f, kToolbarHeight, 0f, 0f));
        }

        private void CreateSettingsMenu() {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Show Guide"), editor.bDrawGuide, editor.ToggleDrawGuide);
            
            menu.DropDown(new Rect(155f, kToolbarHeight, 0f, 0f));
        }

        private void CreateToolsMenu() {

            var menu = new GenericMenu();

            menu.DropDown(new Rect(215f, kToolbarHeight, 0f, 0f));
        }

        public bool DropdownButton(string name, float width) {
            return GUILayout.Button(name, EditorStyles.toolbarDropDown, GUILayout.Width(width));
        }

        public Rect Size { get { return new Rect(Vector2.zero, position.size); } }
        public Rect InputRect {
            get {
                var rect = Size;

                rect.y += kToolbarHeight;
                rect.height -= kToolbarHeight;

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
                    if (w.graph == graphSelected) {
                        return false;
                    }

                    // Found a window with no active canvas.
                    if (w.graph == null) {
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