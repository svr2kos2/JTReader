using System;
using System.Collections.Generic;
using UnityEngine;

namespace DLAT.TxKinSolver {
    public class Crank {
    public List<KinematicJoint> joints = new List<KinematicJoint>();
    Dictionary<int,float> AxisSign = new Dictionary<int,float>();
    Dictionary<int,float> ParentSign = new Dictionary<int,float>();
    Vector3[] linkVectors = new Vector3[4];
    float[] defaultValue = new float[4];
    enum CrankType {
        RRRR,
        PRRR
    }
    CrankType type;

    public bool Contains(KinematicJoint joint) {
        return joints.Contains(joint);
    }

    public void SetCrank(List<KinematicJoint> j) {
        joints = j;
        type = CrankType.RRRR;
        foreach(var joint in joints)
            if(joint.type == 1)
                type = CrankType.PRRR;
        if(type == CrankType.RRRR) {
            SolveRRRRDefault();
        } else {
            SolvePRRRDefault();
        }

        string str = "";

        foreach (var joint in j) {
            str += joint.Name + ", ";
        }
        str += "\n";

        Debug.Log(str + " " + type);

    }

    public void SolveJointUserValue(KinematicJoint joint) {
        if (type == CrankType.RRRR)
            SolveRRRR(joint);
        else
            SolvePRRR(joint);
    }

    void SolveRRRR(KinematicJoint joint) {

        var j = new KinematicJoint[4];
        var v = new Vector3[4];
        var def = new float[4];
        var axSign = new float[4];
        var paSign = new float[4];
        var jointIndex = joints.IndexOf(joint);
        for (var i = jointIndex; i < jointIndex + 4; ++i) {
            j[i - jointIndex] = joints[i % 4];
            v[i - jointIndex] = linkVectors[i % 4];
            def[i - jointIndex] = defaultValue[i % 4];
            axSign[i - jointIndex] = AxisSign[i % 4];
            paSign[i - jointIndex] = ParentSign[i % 4];
        }

        (string, float, float)[] axisInfo = new (string, float, float)[4];
        for (int i = 0; i < 4; ++i)
            axisInfo[i] = (j[i].Name, axSign[i], paSign[i]);
        Array.Sort(axisInfo, (x, y) => { return x.Item1.CompareTo(y.Item1); });

        var b = v[0].magnitude;
        var c = v[1].magnitude;
        //a cross line length
        var a = Mathf.Sqrt(b * b + c * c - 2 * b * c * Mathf.Cos(j[0].sceneValue * axSign[0] * paSign[0] - def[0]));
        var subBAngle = Mathf.Acos((a * a + c * c - b * b) / (2 * a * c));
        var subCAngle = Mathf.Acos((a * a + b * b - c * c) / (2 * a * b));

        b = v[3].magnitude;
        c = v[2].magnitude;
        j[2].sceneValue = -(Mathf.PI + Mathf.Acos((c * c + b * b - a * a) / (2 * b * c))             + def[2]) * axSign[2] * paSign[2];
        j[1].sceneValue = -(Mathf.PI + Mathf.Acos((a * a + b * b - c * c) / (2 * a * b)) + subCAngle + def[1]) * axSign[1] * paSign[1];
        j[3].sceneValue = -(Mathf.PI + Mathf.Acos((a * a + c * c - b * b) / (2 * a * c)) + subBAngle + def[3]) * axSign[3] * paSign[3];

        RepeadUserValue();
    }



    //求空间中两条平行直线, 相连并垂直的向量
    Vector3 GetVecFromParalleVecs(Vector3 fromPoint, Vector3 toPoint, Vector3 dir) {
        var ppVec = toPoint - fromPoint;
        var diffAngle = 90 - Vector3.Angle(dir, ppVec);
        var normal = Vector3.Cross(dir, ppVec).normalized;
        var rotation = Quaternion.AngleAxis(diffAngle, normal);
        return rotation * ppVec * Mathf.Cos(diffAngle * Mathf.Deg2Rad);
    }

    /*
     *     0___1___ 1
     *     /      /
     *   0/      / 2
     *   /______/
     *  3   3   2
     */
    void SolveRRRRDefault() {
        Vector3[] point = new Vector3[4];
        Vector3[] vector = new Vector3[4];
        for (int i = 0; i < 4; ++i) {
            point[i] = joints[i].parent.fixedLink.TransformPoint(joints[i].attachPoint);
            vector[i] = joints[i].parent.fixedLink.TransformVector(joints[i].attachVector).normalized;
        }

        linkVectors[0] = GetVecFromParalleVecs(point[0], point[3], vector[0]);
        linkVectors[1] = GetVecFromParalleVecs(point[0], point[1], vector[0]);
        linkVectors[2] = GetVecFromParalleVecs(point[2], point[1], vector[2]);
        linkVectors[3] = GetVecFromParalleVecs(point[2], point[3], vector[2]);

        var normalAxis = Vector3.Cross(linkVectors[0], linkVectors[1]).normalized;
        AxisSign.Add(0, (vector[0] + normalAxis).magnitude < 0.1f ? 1 : -1);
        AxisSign.Add(1, (vector[1] + normalAxis).magnitude < 0.1f ? 1 : -1);
        AxisSign.Add(2, (vector[2] + normalAxis).magnitude < 0.1f ? 1 : -1);
        AxisSign.Add(3, (vector[3] + normalAxis).magnitude < 0.1f ? 1 : -1);
        ParentSign.Add(0, (joints[0].child == joints[3].child || joints[0].child == joints[3].parent) ? 1 : -1);
        ParentSign.Add(1, (joints[1].child == joints[0].child || joints[1].child == joints[0].parent) ? 1 : -1);
        ParentSign.Add(2, (joints[2].child == joints[1].child || joints[2].child == joints[1].parent) ? 1 : -1);
        ParentSign.Add(3, (joints[3].child == joints[2].child || joints[3].child == joints[2].parent) ? 1 : -1);

        defaultValue[0] = Vector3.Angle(linkVectors[0], linkVectors[1]) * Mathf.Deg2Rad;
        defaultValue[1] = Vector3.Angle(linkVectors[1], linkVectors[2]) * Mathf.Deg2Rad;
        defaultValue[2] = Vector3.Angle(linkVectors[2], linkVectors[3]) * Mathf.Deg2Rad;
        defaultValue[3] = Vector3.Angle(linkVectors[0], linkVectors[3]) * Mathf.Deg2Rad;

    }
        
    void SolvePRRR(KinematicJoint joint) {

        RepeadUserValue();
        var p = joints.IndexOf(joint);
        float A, B, C, a, b, c;
        b = linkVectors[1].magnitude;
        c = linkVectors[2].magnitude;
        if (p == 0) { //Prismatic
            a = joint.sceneValue + defaultValue[0];
            C = Mathf.Acos((a * a + b * b - c * c) / (2 * a * b));
            A = Mathf.Acos((b * b + c * c - a * a) / (2 * b * c));
            B = Mathf.Acos((a * a + c * c - b * b) / (2 * a * c));
        } else if (p == 1) {
            C = joint.sceneValue * AxisSign[1] * ParentSign[1] - defaultValue[1];
            B = Mathf.Asin(Mathf.Sin(C) * b / c);
            A = Mathf.PI - B - C;
            a = Mathf.Sin(A) * b / Mathf.Sin(b);
        } else if (p == 2) {
            A = joint.sceneValue * AxisSign[2] * ParentSign[2] - defaultValue[2];
            a = Mathf.Sqrt(b * b + c * c - 2 * b * c * Mathf.Cos(A));
            B = Mathf.Acos((a * a + c * c - b * b) / (2 * a * c)) * Mathf.Sign(A);
            C = Mathf.Acos((a * a + b * b - c * c) / (2 * a * b)) * Mathf.Sign(A);
        } else {
            B = joint.sceneValue * AxisSign[3] * ParentSign[3] - defaultValue[3];
            C = Mathf.Asin(Mathf.Sin(B) * c / b);
            A = Mathf.PI - B - C;
            a = Mathf.Sin(A) * b / Mathf.Sin(b);
        }
        
        //Debug.Log(A.ToString("f2") + ", " + B.ToString("f2") + ", " + C.ToString("f2") + ", " + Mathf.Abs(A+B+C));
        joints[0].sceneValue =  (a - defaultValue[0]) * 1000f;
        joints[1].sceneValue =  (C + defaultValue[1]) * AxisSign[1] * ParentSign[1];
        joints[2].sceneValue =  (A + defaultValue[2]) * AxisSign[2] * ParentSign[2];
        joints[3].sceneValue =  (B + defaultValue[3]) * AxisSign[3] * ParentSign[3];
        RepeadUserValue();
    }

    void RepeadUserValue() {
        foreach(var j in joints) {
            if (j.type != 0)
                continue;
            var len = 2 * Mathf.PI;
            if (j.userValue < -Mathf.PI)
                j.userValue += Mathf.Ceil((-Mathf.PI - j.userValue) / len) * len;
            if(j.userValue > Mathf.PI)
                j.userValue -= Mathf.Ceil((j.userValue - Mathf.PI) / len) * len;
        }
    }

    /*      A joints[2]
     *      /\
     *   c /  \ b
     *    /    \
     * B /__||__\ C
     *    joints[0]
     *    linkVec[0]
     *    defaul[3]
     *    逆时针
     */
    void SolvePRRRDefault() {
        var p = 0;
        for (int i = 0; i < 4; ++i)
            if (joints[i].type == 1)
                p = i;

        var newJoints = new List<KinematicJoint>();
        for (int i = p; i < p + 4; ++i) {
            newJoints.Add(joints[i % 4]);
        }
        joints = newJoints;

        Vector3[] point = new Vector3[4];
        Vector3[] vector = new Vector3[4];
        for (int i = 0; i < 4; ++i) {
            point[i] = joints[i].parent.fixedLink.TransformPoint(joints[i].attachPoint);
            vector[i] = joints[i].parent.fixedLink.TransformVector(joints[i].attachVector).normalized;
        }
        
        // a b c
        linkVectors[0] = GetVecFromParalleVecs(point[3], point[1], vector[3]);
        linkVectors[1] = GetVecFromParalleVecs(point[1], point[2], vector[1]);
        linkVectors[2] = GetVecFromParalleVecs(point[2], point[3], vector[2]);

        var normalAxis = Vector3.Cross(linkVectors[0], linkVectors[1]).normalized;
        AxisSign.Add(1, (vector[1] + normalAxis).magnitude < 0.1f ? 1 : -1);
        AxisSign.Add(2, (vector[2] + normalAxis).magnitude < 0.1f ? 1 : -1);
        AxisSign.Add(3, (vector[3] + normalAxis).magnitude < 0.1f ? 1 : -1);
        ParentSign.Add(1, (joints[1].child == joints[2].child || joints[1].child == joints[2].parent) ? 1 : -1);
        ParentSign.Add(2, (joints[2].child == joints[3].child || joints[2].child == joints[3].parent) ? 1 : -1);
        ParentSign.Add(3, (joints[3].parent == joints[2].child || joints[3].parent == joints[2].parent) ? 1 : -1);

        defaultValue[0] = linkVectors[0].magnitude;
        //C A B
        defaultValue[1] = Mathf.PI - Vector3.Angle(linkVectors[0], linkVectors[1]) * Mathf.Deg2Rad;
        defaultValue[2] = Mathf.PI - Vector3.Angle(linkVectors[1], linkVectors[2]) * Mathf.Deg2Rad;
        defaultValue[3] = Mathf.PI - Vector3.Angle(linkVectors[2], linkVectors[0]) * Mathf.Deg2Rad;

        //defaultValue[1] *= ParentSign[1];
        //defaultValue[2] *= ParentSign[2];
        //defaultValue[3] *= ParentSign[3];

        //defaultValue[1] *= AxisSign[1];
        //defaultValue[2] *= AxisSign[2];
        //defaultValue[3] *= AxisSign[3];
    }

}

}