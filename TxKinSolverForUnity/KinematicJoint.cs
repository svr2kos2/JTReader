using System.Collections.Generic;
using UnityEngine;

namespace DLAT.TxKinSolver {
    public class KinematicJoint {
        public string Name;
        public int type;
        public KinematicPart parent;
        public KinematicPart child;
        public Vector3 attachPoint;
        public Vector3 attachVector;
        public float userValue;
        public float sceneValueOffset;
        public float sceneValue {
            get {
                return userValue + sceneValueOffset;
            }
            set {
                userValue = value - sceneValueOffset;
            }
        }
        public List<string> function;
        public float MaxLimit { get; set; }
        public float MinLimit { get; set; }

        public bool visted = false;

    }

}