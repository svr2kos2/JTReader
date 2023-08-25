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
            var dataBegin = stream.Position; //debug
            var elementLength = stream.ReadI32();
            var begin = stream.Position;
            objectTypeID = stream.ReadGUID();
            if (elementLength == 16)
                return;
            var baseType = stream.ReadU8();
            if(majorVersion > 8)
                objectID = stream.ReadI32();

            //Debug.cache = true;
            
            dataStream = new MemoryStream(stream.ReadBytes(elementLength - (int)(stream.Position - begin), 1));
            dataStream.FromJTFile(stream.FromJTFile());
            dataStream.Position = 0;
            var elementType = ObjectTypeIdentifiers.types[objectTypeID.ToString()];

            Debug.Log("    Element ID:" + objectID
                                        + " type(#g" + ObjectTypeIdentifiers.GetTypeString(objectTypeID) + "#w) "
                                        + "begin:" + dataBegin + " len:" + elementLength + " end:" +
                                        stream.Position + " dataLen:" + dataStream.Length);

            try {
                elementData = Activator.CreateInstance(elementType, new object[] { this });
                //Debug.cache = false;
            
                //Debug.FlushLogs();
                if (dataStream.Position + 1 == dataStream.Length) return; 
                //read length not equal
                Debug.Log(
                    dataStream.Position < dataStream.Length - 1
                        ? $"#yElement data not fully read! Remain:{dataStream.Length - dataStream.Position - 1}#w"
                        : "#rElement data over read!#w", 2);
            }
            catch (Exception e){
                Debug.Log($"#rRead failed. {e.Message}#w", 2);
            }
            
        }
    }
}
