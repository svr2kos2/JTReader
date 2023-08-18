using JTReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DLAT.JTReader {
    public class Element {
        public DataSegment segment;
        public GUID objectTypeID;
        public int objectID = -1;
        public Stream dataStream;

        public object elementData;

        public int majorVersion {
            get { return segment.majorVersion; }
        }
        public int minorVersion {
            get { return segment.minorVersion; }
        }

        public Element(DataSegment seg) {
            segment = seg;
            var stream = seg.dataStream;
            var elementLength = stream.ReadI32();
            var begin = stream.Position;
            objectTypeID = stream.ReadGUID();
            if (elementLength == 16)
                return;
            var baseType = stream.ReadU8();
            if(majorVersion > 8)
                objectID = stream.ReadI32();

            //Debug.cache = true;
            Debug.Log("    Element ID:" + objectID 
                                        + " type(#g" + ObjectTypeIdentifiers.GetTypeString(objectTypeID) + "#w) len:" + elementLength);
            dataStream = new MemoryStream(stream.ReadBytes(elementLength - (int)(stream.Position - begin)));
            dataStream.ByteOrder(stream.ByteOrder());
            var elementType = ObjectTypeIdentifiers.types[objectTypeID.ToString()];
            elementData = Activator.CreateInstance(elementType, new object[] { this });
            //Debug.cache = false;
            
            //Debug.FlushLogs();
            if (dataStream.Position != dataStream.Length)
                Debug.Log("#rElement data not fully read!#w", 2);
        }
    }
}
