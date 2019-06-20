
using System;
using System.Collections.Generic;
using UnityEngine;

namespace OSM
{
    public abstract class OSM_Graph : ScriptableObject {

        public abstract string GetName();
        public abstract Type GetNodeType();
        public abstract List<OSM_Node> GetNodes();
        
        public abstract void CreateNode(Vector2 position);
        public abstract void Add(OSM_Node n, Vector2 position);
        public abstract void Remove(OSM_Node node);
        public abstract void PushToEnd(OSM_Node node);
        public abstract void OnSave();
        public abstract void Clear();
    }

    public class OSM_Graph<T> : OSM_Graph where T : OSM_Node {

        public List<T> nodes;
        
        public override List<OSM_Node> GetNodes() {
            return nodes as List<OSM_Node>;
        }
        

        public override void CreateNode(Vector2 position) {

            Debug.Log($"Current Nodes Count {nodes.Count}");
        }
        
        public override void Add(OSM_Node n, Vector2 position) {

            // nodes.Add(n);
        }

        public override void Remove(OSM_Node node) {

            // nodes.Remove(node);
        }

        public override void PushToEnd(OSM_Node node) {

            // if (nodes.Remove(node)) {
            //     nodes.Add(node);
            // }
        }

        public override void OnSave() { }

        public override void Clear() {
            foreach (var node in nodes) {
                ScriptableObject.DestroyImmediate(node as OSM_Graph<T>, true);
            }

            nodes.Clear();
        }

        public override string GetName() => name;
        public override Type GetNodeType() => typeof(T);
    }
}