using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLAT.JTReader {
    public class RangeLODNodeData : LODNodeData {
        public List<float> rangeLimits;
        public CoordF32 center;
        public RangeLODNodeData(Element ele) : base(ele) {
            var data = ele.dataStream;
            short version = 0;
            if (ele.majorVersion == 9)
                version = data.ReadI16();
            if (ele.minorVersion == 10)
                version = data.ReadU8();
            rangeLimits = data.ReadVecF32();
            center = new CoordF32(data);
        }
    }
}
