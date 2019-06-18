
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

using NodeEditorFramework.Utilities;

namespace OSM {

    public struct DrawNodeData {

    }

    public class OSM_Editor {
        public static Action<OSM_Graph, OSM_Node> onNodeGuiChange;

        public static readonly Rect kReticleRect = new Rect(0, 0, 8, 8);

        public static float zoomDelta = 0.1f;
        public static float minZoom = 1f;
        public static float maxZoom = 4f;
        public static float panSpeed = 1.2f;

        public OSM_Graph graph;
        private OSM_EditorWindow _window;

        private DrawNodeData nodeWindow;

        private Texture2D _gridTex;
        private Texture2D _backTex;
        private Texture2D _circleTex;

        public Color backColor;
        public Color knobColor;
        public Color guideColor;

        private Vector2 _zoomAdjustment;
        private Vector2 _zoom = Vector2.one;
        public Vector2 panOffset = Vector2.zero;

        public bool bDrawGuide = false;

        public OSM_Editor(OSM_EditorWindow w) {
            backColor = new Color32(59, 62, 74, 255);
            knobColor = new Color32(126, 186, 255, 255);
            
            guideColor = Color.gray;
            guideColor.a = 0.3f;

            _gridTex = Resources.Load("Editor/Grid") as Texture2D;
            _backTex = Resources.Load("Editor/Square") as Texture2D;
            _circleTex = Resources.Load("Editor/Circle") as Texture2D;

            _window = w;
        }

        #region Drawing

        public void Draw() {
            if (Event.current.type == EventType.Repaint) {
                DrawGrid();
                updateTextures();
            }

            if (graph != null)
                DrawGraphContents();

            DrawMode();
            DrawToolbar();
        }

        private void DrawToolbar() {
            EditorGUILayout.BeginHorizontal("Toolbar");

            if (DropdownButton("File")) { FileMenuContext(); }
            if (DropdownButton("Edit")) { EditMenuContext(); }
            if (DropdownButton("View")) { ViewMenuContext(); }
            if (DropdownButton("Settings")) { SettingsMenuContext(); }
            if (DropdownButton("Tools")) { ToolsMenuContext(); }

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

        private void FileMenuContext() {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Open"), false, () => { _window.FindGraph(); });
            menu.DropDown(new Rect(5f, _window.ToolbarHeight, 0f, 0f));
        }

        private void EditMenuContext() {
            var menu = new GenericMenu();
            menu.DropDown(new Rect(55f, _window.ToolbarHeight, 0f, 0f));
        }

        private void ViewMenuContext() {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Home"), false, HomeView);
            menu.AddItem(new GUIContent("Zoom In"), false, () => { Zoom(-1); });
            menu.AddItem(new GUIContent("Zoom Out"), false, () => { Zoom(1); });

            menu.DropDown(new Rect(105f, _window.ToolbarHeight, 0f, 0f));
        }

        private void SettingsMenuContext() {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Show Guide"), bDrawGuide, ToggleDrawGuide);
            menu.DropDown(new Rect(155f, _window.ToolbarHeight, 0f, 0f));
        }

        private void ToolsMenuContext() {
            var menu = new GenericMenu();
            menu.DropDown(new Rect(215f, _window.ToolbarHeight, 0f, 0f));
        }

        public bool DropdownButton(string name) {
            return GUILayout.Button(name, EditorStyles.toolbarDropDown, GUILayout.Width(_window.ToolbarButtonWidth));
        }

        private void DrawGraphContents() {
            Rect graphRect = _window.Size;
            var center = graphRect.size / 2f;

            _zoomAdjustment = GUIScaleUtility.BeginScale(ref graphRect, center, ZoomScale, false);

            DrawGridOverlay();
            DrawConnectionPreview();
            DrawConnections();
            DrawNodes();

            GUIScaleUtility.EndScale();
        }

        private void DrawGrid()
        {
            var size = _window.Size.size;
            var center = size / 2f;

            float zoom = ZoomScale;

            // Offset from origin in tile units
            float xOffset = -(center.x * zoom + panOffset.x) / _gridTex.width;
            float yOffset = ((center.y - size.y) * zoom + panOffset.y) / _gridTex.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(size.x * zoom) / _gridTex.width;
            float tileAmountY = Mathf.Round(size.y * zoom) / _gridTex.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(_window.Size, _gridTex, new Rect(tileOffset, tileAmount));
        }

        // Handles Drawing things over the grid such as axes.
        private void DrawGridOverlay() {
            DrawAxes();
            DrawGridCenter();

            if (bDrawGuide) {
                DrawGuide();
                _window.Repaint();
            }
        }

        private void DrawGridCenter() {
            var rect = kReticleRect;

            rect.size *= ZoomScale;
            rect.center = Vector2.zero;
            rect.position = GraphToScreenSpace(rect.position);
            
            DrawTintTexture(rect, _circleTex, Color.gray);
        }

        private void DrawAxes() {
            // Draw axes. Make sure to scale based on zoom.
            Vector2 up = Vector2.up * _window.Size.height * ZoomScale;
            Vector2 right = Vector2.right * _window.Size.width * ZoomScale;
            Vector2 down = -up;
            Vector2 left = -right;

            // Make sure the axes follow the pan.
            up.y -= panOffset.y;
            down.y -= panOffset.y;
            right.x -= panOffset.x;
            left.x -= panOffset.x;

            up = GraphToScreenSpace(up);
            down = GraphToScreenSpace(down);
            right = GraphToScreenSpace(right);
            left = GraphToScreenSpace(left);

            DrawLine(right, left, Color.gray);
            DrawLine(up, down, Color.gray);
        }

        private void DrawGuide() {
            Vector2 gridCenter = GraphToScreenSpace(Vector2.zero);
            DrawLine(gridCenter, Event.current.mousePosition, guideColor);
        }

        private void DrawNodes() {
            //
        }

        private void DrawConnections() { }

        private void DrawConnectionPreview() {
            var output = _window.state.selectedOutput;

            if (output != null) {
                Vector2 start = GraphToScreenSpace(output.bodyRect.center);
                DrawBezier(start, Event.current.mousePosition, Color.gray);
            }
        }

        private void DrawNode(OSM_Node node) {
            // Convert the node rect from graph to screen space.
            Rect screenRect = node.bodyRect;
            screenRect.position = GraphToScreenSpace(screenRect.position);

            // The node contents are grouped together within the node body.
            BeginGroup(screenRect, backgroundStyle, backColor);

            // Make the body of node local to the group coordinate space.
            Rect localRect = node.bodyRect;
            localRect.position = Vector2.zero;

            // Draw the contents inside the node body, automatically laidout.
            GUILayout.BeginArea(localRect, GUIStyle.none);

            // node.HeaderStyle.normal.background = _headerTex;

            // EditorGUI.BeginChangeCheck();
            // node.OnNodeGUI();
            // if (EditorGUI.EndChangeCheck()) 
            //     if (onNodeGuiChange != null) onNodeGuiChange(graph, node);

            // GUILayout.EndArea();
            // GUI.EndGroup();
        }

        public void DrawMode() {

            if (!graph) {
                GUI.Label(_modeStatusRect, new GUIContent("No Graph Set"), ModeStatusStyle);
            }

            else if (_window.GetMode() == OSM_EditorWindow.Mode.Edit) {
                GUI.Label(_modeStatusRect, new GUIContent("Edit"), ModeStatusStyle);
            }

            else {
                GUI.Label(_modeStatusRect, new GUIContent("View"), ModeStatusStyle);
            }
        }

        public static void DrawBezier(Vector2 start, Vector2 end, Color color) {
            Vector2 endToStart = (end - start);
            float dirFactor = Mathf.Clamp(endToStart.magnitude, 20f, 80f);

            endToStart.Normalize();
            Vector2 project = Vector3.Project(endToStart, Vector3.right);

            Vector2 startTan = start + project * dirFactor;
            Vector2 endTan = end - project * dirFactor;

            UnityEditor.Handles.DrawBezier(start, end, startTan, endTan, color, null, 3f);
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color) {
            var handleColor = Handles.color;
            Handles.color = color;

            Handles.DrawLine(start, end);
            Handles.color = handleColor;
        }

        public static void DrawTintTexture(Rect r, Texture t, Color c) {
            var guiColor = GUI.color;
            GUI.color = c;

            GUI.DrawTexture(r, t);
            GUI.color = guiColor;
        }

        public static void BeginGroup(Rect r, GUIStyle style, Color color) {
            var old = GUI.color;

            GUI.color = color;
            GUI.BeginGroup(r, style);

            GUI.color = old;
        }

        // TODO: Call after exiting playmode.
        private void updateTextures() {
            // _knobTex = TextureLib.GetTintTex("Circle", knobColor);
            // _headerTex = TextureLib.GetTintTex("Square", ColorExtensions.From255(79, 82, 94));
        }

        #endregion

        #region View Operations

        public void ToggleDrawGuide() {
            bDrawGuide = !bDrawGuide;
        }

        public void HomeView() {
            // if (!graph || graph.nodes.Count == 0) {
            //     panOffset = Vector2.zero;
            //     return;
            // }

            // float xMin = float.MaxValue;
            // float xMax = float.MinValue;
            // float yMin = float.MaxValue;
            // float yMax = float.MinValue;

            // foreach (var node in graph.nodes) {

            //     Rect r = node.bodyRect;

            //     if (r.xMin < xMin) {
            //         xMin = r.xMin;
            //     }

            //     if (r.xMax > xMax) {
            //         xMax = r.xMax;
            //     }

            //     if (r.yMin < yMin) {
            //         yMin = r.yMin;
            //     }

            //     if (r.yMax > yMax) {
            //         yMax = r.yMax;
            //     }
            // }

            // // Add some padding so nodes do not appear on the edge of the view.
            // xMin -= Node.kDefaultSize.x;
            // xMax += Node.kDefaultSize.x;
            // yMin -= Node.kDefaultSize.y;
            // yMax += Node.kDefaultSize.y;
            // var nodesArea = Rect.MinMaxRect(xMin, yMin, xMax, yMax);

            // // Center the pan in the bounding view.
            // panOffset = -nodesArea.center;

            // // Calculate the required zoom based on the ratio between the window view and node area rect.
            // var winSize = _window.Size;
            // float zoom = 1f;

            // // Use the view width to determine zoom to fit the entire node area width.
            // if (nodesArea.width > nodesArea.height) {

            //     float widthRatio = nodesArea.width / winSize.width;
            //     zoom = widthRatio;

            //     if (widthRatio < 1f) {
            //         zoom = 1 / widthRatio;
            //     }
            // }

            // // Use the height to determine zoom.
            // else {

            //     float heightRatio = nodesArea.height / winSize.height;
            //     zoom = heightRatio;

            //     if (heightRatio < 1f) {
            //         zoom = 1 / heightRatio;
            //     }
            // }

            // ZoomScale = zoom;
        }

        #endregion

        #region Space Transformations and Mouse Utilities

        public void Pan(Vector2 delta) {
            panOffset += delta * ZoomScale * panSpeed;
        }

        public void Zoom(float zoomDirection) {
            float scale = (zoomDirection < 0f) ? (1f - zoomDelta) : (1f + zoomDelta);

            _zoom *= scale;

            float cap = Mathf.Clamp(_zoom.x, minZoom, maxZoom);
            _zoom.Set(cap, cap);
        }

        public float ZoomScale {
            get { return _zoom.x; }
            set {
                float z = Mathf.Clamp(value, minZoom, maxZoom);
                _zoom.Set(z, z);
            }
        }

        public Vector2 ScreenToGraphSpace(Vector2 screenPos) {

            var graphRect = _window.Size;
            var center = graphRect.size / 2f;
            return (screenPos - center) * ZoomScale - panOffset;
        }

        public Vector2 MousePosition() {
            return ScreenToGraphSpace(Event.current.mousePosition);
        }

        public bool IsUnderMouse(Rect r) {
            return r.Contains(MousePosition());
        }

        public Vector2 GraphToScreenSpace(Vector2 graphPos) {
            return graphPos + _zoomAdjustment + panOffset;
        }

        public void graphToScreenSpace(ref Vector2 graphPos) {
            graphPos += _zoomAdjustment + panOffset;
        }

        public void graphToScreenSpaceZoomAdj(ref Vector2 graphPos) {
            graphPos = GraphToScreenSpace(graphPos) / ZoomScale;
        }

        // private bool isMouseOverNode() {
        //     return OnMouseOverNode(onSingleSelected);
        // }

        // private bool isMouseOverCanvas()
        // {
        //     return !isMouseOverNode();
        // }

        // private bool isMouseOverOutput()
        // {
        //     return window.editor.OnMouseOverOutput(onOutputKnobSelected);
        // }

        #endregion

        #region Styles

        private GUIStyle _backgroundStyle;
        private GUIStyle backgroundStyle {
            get {
                if (_backgroundStyle == null) {
                    _backgroundStyle = new GUIStyle(GUI.skin.box);
                    _backgroundStyle.normal.background = _backTex;
                }

                return _backgroundStyle;
            }
        }

        private static Rect _modeStatusRect = new Rect(20f, 20f, 250f, 150f);
        private static GUIStyle _modeStatusStyle;
        private static GUIStyle ModeStatusStyle {
            get {
                if (_modeStatusStyle == null) {
                    _modeStatusStyle = new GUIStyle();
                    _modeStatusStyle.fontSize = 36;
                    _modeStatusStyle.fontStyle = FontStyle.Bold;
                    _modeStatusStyle.normal.textColor = new Color(1f, 1f, 1f, 0.2f);
                }

                return _modeStatusStyle;
            }
        }

        #endregion
    }
}
