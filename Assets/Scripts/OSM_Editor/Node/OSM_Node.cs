
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace OSM
{
    public abstract class OSM_Node : ScriptableObject {
        public static readonly Vector2 kDefaultSize = new Vector2(140f, 110f);

        public const float kKnobOffset = 4f;

        public const float kHeaderHeight = 15f;

        public const float kBodyLabelWidth = 100f;

        [HideInInspector]
        public Rect bodyRect;

        public const float resizePaddingX = 20f;

        [SerializeField, HideInInspector]
        protected List<NodeOutput> _outputs = new List<NodeOutput>();

        [SerializeField, HideInInspector]
        protected List<NodeInput> _inputs = new List<NodeInput>();

        public IEnumerable<NodeOutput> Outputs { get { return _outputs; } }

        public IEnumerable<NodeInput> Inputs { get { return _inputs; } }

        public int InputCount { get { return _inputs.Count; } }
        public int OutputCount { get { return _outputs.Count; } }
        public NodeInput GetInput(int index) { return _inputs[index]; }
        public NodeOutput GetOutput(int index) { return _outputs[index]; }

        public abstract void OnConnectionsGUI();
        public abstract void OnInputConnectionRemoved(NodeInput removedInput);
        public abstract void OnNewInputConnection(NodeInput addedInput);
    }
    
    public class OSM_Node<T> : OSM_Node
    {
        // Hides the node asset.
        // Sets up the name via type information.
        void OnEnable()
        {
            hideFlags = HideFlags.HideInHierarchy;
            name = GetType().Name;

#if UNITY_EDITOR
            name = ObjectNames.NicifyVariableName(name);
#endif

        }

        protected virtual void OnDestroy()
        {
            _inputs.RemoveAll(
                (input) =>
                {
                    ScriptableObject.DestroyImmediate(input, true);
                    return true;
                });

            _outputs.RemoveAll(
                (output) =>
                {
                    ScriptableObject.DestroyImmediate(output, true);
                    return true;
                });
        }

        public virtual void Init() {
            bodyRect.size = kDefaultSize;
        }

        public virtual void OnNodeGUI()
        {
            OnNodeHeaderGUI();
            OnConnectionsGUI();
            onBodyGuiInternal();
        }

        public override void OnConnectionsGUI()
        {
            int inputCount = _inputs.Count;
            int outputCount = _outputs.Count;

            int maxCount = (int)Mathf.Max(inputCount, outputCount);

            // The entire knob section is stacked rows of inputs and outputs.
            for (int i = 0; i < maxCount; ++i) {

                GUILayout.BeginHorizontal();

                // Render the knob layout horizontally.
                if (i < inputCount) _inputs[i].OnConnectionGUI(i);
                if (i < outputCount) _outputs[i].OnConnectionGUI(i);

                GUILayout.EndHorizontal();
            }
        }

        public virtual void OnNodeHeaderGUI()
        {
            // Draw header
            GUILayout.Box(name, HeaderStyle);
        }

        public virtual void OnBodyGUI() { }

        protected virtual void onBodyGuiInternal()
        {
            float oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = kBodyLabelWidth;

            var oldLabelStyle = UnityLabelStyle;

            EditorStyles.label.normal = DefaultStyle.normal;
            EditorStyles.label.active = DefaultStyle.active;
            EditorStyles.label.focused = DefaultStyle.focused;

            EditorGUILayout.BeginVertical();

            GUILayout.Space(kKnobOffset);
            OnBodyGUI();

            EditorStyles.label.normal = oldLabelStyle.normal;
            EditorStyles.label.active = oldLabelStyle.active;
            EditorStyles.label.focused = oldLabelStyle.focused;

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUILayout.EndVertical();
        }

        public NodeInput AddInput(string name = "input", bool multipleConnections = false)
        {
            var input = NodeInput.Create(this, multipleConnections);
            input.name = name;
            _inputs.Add(input);

            return input;
        }

        public NodeOutput AddOutput(string name = "output", bool multipleConnections = false)
        {
            var output = NodeOutput.Create(this, multipleConnections);
            output.name = name;
            _outputs.Add(output);

            return output;
        }

        public override void OnInputConnectionRemoved(NodeInput removedInput) { }

        public override void OnNewInputConnection(NodeInput addedInput) { }


        public float HeaderTop
        {
            get { return bodyRect.yMin + kHeaderHeight; }
        }

        public void FitKnobs()
        {
            int maxCount = (int)Mathf.Max(_inputs.Count, _outputs.Count);

            float totalKnobsHeight = maxCount * NodeConnection.kMinSize.y;
            float totalOffsetHeight = (maxCount - 1) * kKnobOffset;

            float heightRequired = totalKnobsHeight + totalOffsetHeight + kHeaderHeight;

            // Add some extra height at the end.
            bodyRect.height = heightRequired + kHeaderHeight / 2f;
        }

        #region Styles and Contents

        private static GUIStyle _unityLabelStyle;

        public static GUIStyle UnityLabelStyle
        {
            get
            {
                if (_unityLabelStyle == null) {
                    _unityLabelStyle = new GUIStyle(EditorStyles.label);
                }

                return _unityLabelStyle;
            }
        }

        private static GUIStyle _defStyle;
        public static GUIStyle DefaultStyle {
            get {
                if (_defStyle == null) {
                    _defStyle = new GUIStyle(EditorStyles.label);
                    _defStyle.normal.textColor = Color.white * 0.9f;
                    _defStyle.active.textColor = new Color32(126, 186, 255, 255);
                    _defStyle.focused.textColor = new Color32(126, 186, 255, 255);
                }

                return _defStyle;
            }
        }

        private static GUIStyle _headerStyle;
        public GUIStyle HeaderStyle
        {
            get
            {
                if (_headerStyle == null) {

                    _headerStyle = new GUIStyle();

                    _headerStyle.stretchWidth = true;
                    _headerStyle.alignment = TextAnchor.MiddleLeft;
                    _headerStyle.padding.left = 5;
                    _headerStyle.normal.textColor = Color.white * 0.9f;
                    _headerStyle.fixedHeight = kHeaderHeight;
                }

                return _headerStyle;
            }
        }

        #endregion
    }
}