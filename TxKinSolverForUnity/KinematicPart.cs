using System.Collections.Generic;
using UnityEngine;

namespace DLAT.TxKinSolver {
    public class KinematicPart {
    public TxKinSolver _solver;
    public string id;
    public Transform fixedLink;
    public string ExternalID;
    //public List<Transform> links = new List<Transform>();
    //public List<KinematicJoint> joints = new List<KinematicJoint>();
    public List<KinematicJoint> parentJoints = new List<KinematicJoint>();
    public List<KinematicJoint> childJoints = new List<KinematicJoint>();
    public bool visted = false;

    public static KinematicPart Create(TxKinSolver solver, Transform root, Transform link) {
        if(link == null) {
            link = new GameObject("dummyLink").transform;
            link.SetParent(root);
            link.transform.localPosition = Vector3.zero;
            link.transform.localRotation = Quaternion.identity;
        }
            
        var part = new KinematicPart();
        part._solver = solver;
        part.fixedLink = link;
        part.id = link.name;
        return part;
    }

    float GetRevoluteUserValue(Transform parent, Transform t, Vector3 tAxis) {
        var rotation = Quaternion.Inverse(parent.rotation) * t.rotation;
        Vector3 axis;
        float angle;
        rotation.ToAngleAxis(out angle, out axis);
        //Debug.DrawRay(parent.TransformPoint(attach),parent.TransformDirection(axis),Color.red);
        //Debug.DrawRay(parent.TransformPoint(attach),parent.TransformDirection(tAxis),Color.green);
        var res = Vector3.Dot(tAxis.normalized, axis.normalized) * angle;
        return res * Mathf.Deg2Rad;
    }
    void ApplyeUserValue(KinematicJoint joint) {
        var parent = joint.parent.fixedLink;
        var t = joint.child.fixedLink;
        var tPoint = joint.attachPoint;
        var tAxis = joint.attachVector;
        float currentValue;
        float diff;
        switch (joint.type) {
            case 0:
                float targetValue = joint.sceneValue;
                targetValue = -targetValue;
                currentValue = GetRevoluteUserValue(parent, t, tAxis);
                diff = targetValue - currentValue;
                t.RotateAround(t.TransformPoint(tPoint), t.TransformVector(tAxis), diff * Mathf.Rad2Deg);
                break;
            case 1:
                currentValue = Vector3.Distance(parent.transform.TransformPoint(tPoint), t.transform.TransformPoint(tPoint));
                diff = joint.sceneValue * 0.001f - currentValue;
                t.Translate(t.TransformVector(tAxis).normalized * diff, Space.World);
                break;
        };
    }

    public void Solve() {
        visted = true;
        foreach (var joint in childJoints) {
            if (joint.child.visted)
                continue;
            var t = joint.child.fixedLink;
            Vector3 point = joint.attachPoint;
            Vector3 vector = joint.attachVector;
            var fVector = fixedLink.TransformVector(vector);
            var tVector = t.TransformVector(vector);
            var normal = Vector3.Cross(fVector, tVector).normalized;
            t.Rotate(-normal, Vector3.SignedAngle(fVector, tVector, normal), Space.World);
            var fPoint = fixedLink.TransformPoint(point);
            var tPoint = t.TransformPoint(point);
            t.Translate(fPoint - tPoint, Space.World);
            //DrawArrow.ForDebug(tPoint, tVector, Color.red);
            //Debug.DrawRay(fPoint, fVector, Color.red);
            //Debug.DrawRay(tPoint, tVector, Color.red);
            if (joint.function != null)
                joint.userValue = JointFunctionSolver.Solve(_solver.kinematicJointList, joint.function);
            ApplyeUserValue(joint);
            joint.child.Solve();
        }
        visted = false;
    }
}

}