using System.Collections.Generic;
using DLAT.JTReader;
using UnityEngine;

namespace DLAT.TxKinSolver {
    public static class Utility {
        public class MergeSet<T> {
            Dictionary<T, T> parents = new Dictionary<T, T>();
            bool compress = true;

            public MergeSet(bool compressWhenGet = true) {
                compress = compressWhenGet;
            }

            public T Get(T index) {
                if (!parents.ContainsKey(index))
                    parents.Add(index, index);
                if (parents[index].Equals(index))
                    return index;
                T p = Get(parents[index]);
                if (compress)
                    parents[index] = p;
                return p;
            }

            public void Merger(T left, T right) {
                T lp = Get(left);
                T rp = Get(right);
                if (!lp.Equals(rp))
                    parents[rp] = lp;
            }
        }
        public static Vector3 ToVector3(this Axis axis) {
            return new Vector3((float)axis.x, (float)axis.y, (float)axis.z);
        }
    }
}