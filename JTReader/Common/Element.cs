﻿using JTReader;
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
        public Type elementType;
        public object elementData;

        public int majorVersion => segment.majorVersion;
        public int minorVersion => segment.minorVersion;

        public void Instantiate() {
            if (dataStream == null)
                return;
            elementData = Activator.CreateInstance(elementType, new object[] { this });
            dataStream = null;
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
            
            //dataStream = stream;
            dataStream = new MemoryStream(stream.ReadBytes(elementLength - (int)(stream.Position - begin), 1));
            dataStream.FromJTFile(stream.FromJTFile());
            dataStream.Position = 0;
            if (!ObjectTypeIdentifiers.types.ContainsKey(objectTypeID.ToString())) {
                dataStream = null;   
                return;
            }
            elementType = ObjectTypeIdentifiers.types[objectTypeID.ToString()];
            
            Instantiate();
            
            if (elementType == typeof(PropertyProxyMetaData)) {
                var data = elementData;
            }
            

            //Debug.Log("    Element ID:" + objectID
            //                            + " type(#g" + ObjectTypeIdentifiers.GetTypeString(objectTypeID) + "#w) "
            //                            + "begin:" + dataBegin + " len:" + elementLength + " end:" +
            //                            stream.Position + " dataLen:" + dataStream.Length);



            //Debug.cache = false;

            //Debug.FlushLogs();
            //if (dataStream.Position + 1 == dataStream.Length - dataBegin) return;

            // var elementInfoStr = "Element ID:" + objectID
            //                              + " type(#g" + ObjectTypeIdentifiers.GetTypeString(objectTypeID) + "#y)#w\n"
            //                              + "    begin:" + dataBegin + " len:" + elementLength + " end:" +
            //                              stream.Position + " dataLen:" + dataStream.Length;
            // //read length not equal
            // Debug.Log(
            //     dataStream.Position < dataStream.Length - 1
            //         ? $"#y{elementInfoStr}. Element data not fully read! Remain:{dataStream.Length - dataStream.Position - 1}#w"
            //         : $"#r{elementInfoStr}Element data over read!#w");
            // if(dataStream.Position >= dataStream.Length - dataBegin - 1)
            //     throw new Exception("Read Align Error");
        }
    }
}
