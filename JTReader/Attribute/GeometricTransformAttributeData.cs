using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLAT.JTReader {
    public class GeometricTransformAttributeData : BaseAttributeData {
        public Mx4F64 matrix;
        public GeometricTransformAttributeData(Element ele) : base(ele) {
            var data = ele.dataStream;
            short version = 0;
            if (ele.majorVersion == 9)
                version = data.ReadI16();
            else if (ele.majorVersion == 10)
                version = data.ReadU8();
            ushort storedValuesMask = data.ReadU16();
            matrix = ele.majorVersion < 10 ? new Mx4F32(data,storedValuesMask).ToMx4F64() : new Mx4F64(data, storedValuesMask);
            base.ReadBaseAttributeDataFields2(data);
        }

        //public UnityEngine.Matrix4x4 GetMatrix() {
        //    var res = new UnityEngine.Matrix4x4 {
        //        m00 = elementValue[15],
        //        m01 = elementValue[14],
        //        m02 = elementValue[13],
        //        m03 = elementValue[12],
        //        m10 = elementValue[11],
        //        m11 = elementValue[10],
        //        m12 = elementValue[9],
        //        m13 = elementValue[8],
        //        m20 = elementValue[7],
        //        m21 = elementValue[6],
        //        m22 = elementValue[5],
        //        m23 = elementValue[4],
        //        m30 = elementValue[3],
        //        m31 = elementValue[2],
        //        m32 = elementValue[1],
        //        m33 = elementValue[0],
        //    };
        //    return res;
        //}
    }
}
