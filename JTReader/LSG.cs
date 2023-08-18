using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLAT.JTReader {
    public class LSG {
        public LSG(DataSegment seg) {
            var elements = new List<Element>();
            for (bool readAttrEle = false; ;) {
                var ele = new Element(seg);
                //Console.WriteLine(ObjectTypeIdentifiers.GetTypeString(ele.objectTypeID));
                if (ObjectTypeIdentifiers.isEOE(ele.objectTypeID)) {
                    if(readAttrEle || seg.majorVersion > 9)
                        break;
                    readAttrEle = true;
                }
                elements.Add(ele);
            }
        }
    }
}
