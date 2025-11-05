using System;
using System.Collections.Generic;
using UnityEngine;

namespace LJ2025
{
    public class DragAlong : MonoBehaviour
    {
        private struct DragObjInfo
        {
            public Vector3 offset;
            public float time;

            public DragObjInfo(Vector3 offset)
            {
                this.offset = offset;
                this.time = Time.time;
            }
        }

        private Dictionary<Transform, DragObjInfo> _objs = new();
        
        private void OnCollisionStay(Collision other)
        {
            if (!_objs.TryGetValue(other.transform, out DragObjInfo info))
            {
                Vector3 offset = other.transform.position - transform.position;
                info = new DragObjInfo(offset);
                _objs.Add(other.transform, info);
            }
            info.time = Time.time;
            other.transform.position = transform.position + info.offset;
        }
        
        List<Transform> _toRemove = new();
        public void Update()
        {
            _toRemove.Clear();
            foreach (var (trans, info) in _objs)
            {
                if (Time.time - info.time > 1) _toRemove.Add(trans);
            }

            foreach (Transform t in _toRemove)
            {
                _objs.Remove(t);
            }
        }
    }
}
