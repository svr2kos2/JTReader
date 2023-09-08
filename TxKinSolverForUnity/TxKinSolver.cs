using System.Collections.Generic;
using System.Linq;
using DLAT.JTReader;
using UnityEngine;

namespace DLAT.TxKinSolver {
    public class TxKinSolver {
        public TxKinDevice device;
        KinematicPart rootPart;
        public Dictionary<int, KinematicPart> kinematicPartList = new Dictionary<int, KinematicPart>();
        public Dictionary<int, KinematicJoint> kinematicJointList = new Dictionary<int, KinematicJoint>();
        List<Crank> cranks = new List<Crank>();
        
        public void SetKinetics(TxKinDevice dev) {
            device = dev;
            foreach (var joint in device.joints) {
                var name = joint.key;
                var parentID = joint.parentLink;
                var childID = joint.childLink;
                var type = joint.jointType;
                var kj = new KinematicJoint();
                kj.Name = name;
                kj.type = type;
                kj.MaxLimit = (float)joint.jointRange.highLimit;
                kj.MinLimit = (float)joint.jointRange.lowLimit;

                if(!kinematicPartList.ContainsKey(parentID)) 
                    kinematicPartList.Add(parentID, KinematicPart.Create(this, transform, parentID));
                kinematicPartList[parentID].childJoints.Add(kj);
                kj.parent = kinematicPartList[parentID];
                kinematicPartList[parentID].ExternalID = device.ExternalID;

                if (!kinematicPartList.ContainsKey(childID)) 
                    kinematicPartList.Add(childID, KinematicPart.Create(this, transform, childID));
                kinematicPartList[childID].parentJoints.Add(kj);
                kj.child = kinematicPartList[childID];
                kinematicPartList[childID].ExternalID = device.ExternalID;

                kj.attachPoint = joint.axis1.ToVector3() * 0.001f;
                kj.attachVector = joint.axis2.ToVector3() * 0.001f - kj.attachPoint;
                kj.userValue = (float)joint.userValue;
                kj.sceneValueOffset = -kj.userValue;
                kj.function = JointFunctionSolver.ConvertToPostfix(joint.function);
                kinematicJointList.Add(kj.Name, kj);
            }
            rootPart = FindRoot();
            SearchCranks(rootPart);
            
        }
        
        
        
        KinematicPart FindRoot() {
            HashSet<KinematicPart> cnt = new HashSet<KinematicPart>();
            foreach (var joint in kinematicJointList) {
                if (!cnt.Contains(joint.Value.child))
                    cnt.Add(joint.Value.child);
            }

            foreach (var i in kinematicPartList)
                if (!cnt.Contains(i.Value))
                    return i.Value;
            return null;
        }
        void SearchCranks(KinematicPart root) {
            var crankJoints = GetCranks(kinematicJointList.Values.ToList());
            foreach (var crank in crankJoints) {
                List<KinematicJoint> joints = new List<KinematicJoint>();
                foreach (var joint in crank)
                    joints.Add(joint);
                var c = new Crank();
                c.SetCrank(crank);
                cranks.Add(c);
            }
        }
        List<List<KinematicJoint>> GetCranks(List<KinematicJoint> jointList) {
            List<List<KinematicJoint>> cranks = new List<List<KinematicJoint>>();

            var ms = new Utility.MergeSet<KinematicPart>(false);
            var conflictJoints = new List<KinematicJoint>();
            foreach (var joint in jointList) {
                if (ms.Get(joint.child) == joint.child)
                    ms.Merger(joint.parent, joint.child);
                else //添加到冲突列表
                    conflictJoints.Add(joint);
            }

            var linkDeepth = new Dictionary<string, int>();
            var que = new Queue<(KinematicPart, int)>();
            var root = jointList.Find((x) => x.parent.parentJoints.Count == 0);
            que.Enqueue((root.parent, 0));
            while (que.Count != 0) {
                var l = que.Dequeue();
                if (linkDeepth.ContainsKey(l.Item1.id))
                    continue;
                linkDeepth.Add(l.Item1.id, l.Item2);
                foreach (var child in l.Item1.childJoints)
                    que.Enqueue((child.child, l.Item2 + 1));
            }


            //查找LCA并储存路径
            foreach (var joint in conflictJoints) {
                var parentPath = new List<KinematicJoint> { joint };
                var childPath = new List<KinematicJoint>();

                var pl = joint.parent;
                var cl = joint.child;
                while (pl != cl) {
                    if (linkDeepth[pl.id] > linkDeepth[cl.id]) {
                        var j = pl.parentJoints.First();
                        pl = pl.parentJoints.First().parent;
                        parentPath.Add(j);
                    }
                    else {
                        var j = cl.parentJoints.First();
                        cl = cl.parentJoints.First().parent;
                        childPath.Add(j);
                    }
                }

                bool crm = false, prm = false;
                while (childPath.Count > 0 && parentPath.Count + childPath.Count > 4) {
                    childPath.RemoveAt(childPath.Count - 1);
                    crm = true;
                }

                while (parentPath.Count > 0 && parentPath.Count + childPath.Count > 4) {
                    parentPath.RemoveAt(parentPath.Count - 1);
                    prm = true;
                }

                if (crm) {
                    if (parentPath.Count != 0)
                        parentPath.Last().parent = childPath.Last().parent;
                    else
                        childPath.First().parent = childPath.Last().parent;
                }

                if (prm) {
                    if (childPath.Count != 0)
                        childPath.Last().parent = parentPath.Last().parent;
                    else
                        parentPath.First().parent = parentPath.First().parent;
                    //如果crm和prm都走这条分支的话应该是swap()
                }

                parentPath.Reverse();
                var crank = new List<KinematicJoint>();
                crank.AddRange(parentPath);
                crank.AddRange(childPath);
                cranks.Add(crank);
            }

            return cranks;
        }

        
    }
}