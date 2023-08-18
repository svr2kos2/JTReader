using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLAT.JTReader {
    public class PartNodeData : MetaDataNodeData {
        public PartNodeData(Element ele) : base(ele) {
            var data = ele.dataStream;
            short version = 0;
            if (version > 9)
                version = data.ReadU8();
            else
                version = data.ReadI16();
            version = data.ReadI16();
            var emptyField = data.ReadI32();
        }
    }
}
