using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
using Joveler.Compression.XZ;
using System.Diagnostics;
using JTReader.Common;

namespace DLAT.JTReader {
    public class DataSegment {
        public JTFile file;
        public GUID segmentID;
        public int segmentType;
        public uint segmentLength;
        public Stream dataStream;

        public List<Element> elements;

        public int majorVersion {
            get {
                return file.majorVersion;
            }
        }
        public int minorVersion {
            get {
                return file.minorVersion;
            }
        }
        
        public DataSegment(JTFile jtFile, int segIndex) {
            file = jtFile;
            var fb = file.fileBytes;
            var pos = 4 + jtFile.tocOffset + segIndex * jtFile.tocEntryLength;
            //----TOC Entry----
            segmentID = fb.ReadGUID(ref pos);
            var segmentOffset = majorVersion > 9 ? fb.ReadI64(ref pos) : fb.ReadI32(ref pos);
            segmentLength = fb.ReadU32(ref pos);
            var segmentAttribute = fb.ReadBytes(ref pos, 4);
            segmentType = segmentAttribute[3];
            //----TOC Entry----

            //----Segment Header----
            pos = segmentOffset;
            if (segmentID != fb.ReadGUID(ref pos))
                throw new Exception("TOC Entry Segment ID doesn't equal to Segment Header Segment ID");
            if (segmentType != fb.ReadI32(ref pos))
                throw new Exception("TOC Entry Segment Type doesn't equal to Segment Header Segment Type");
            if (segmentLength != fb.ReadI32(ref pos))
                throw new Exception("TOC Entry Segment Length doesn't equal to Segment Header Segment Length");
            //----Segment Header----

            if (segmentType == 0) {
                Debug.Log("#rFound segment type 0, skipped.#w");
                return;
            }
            
            var compressed = false;
            var supportCompress = SegmentTypes.GetType(segmentType).Item2;
            if(supportCompress) {
                var compressionFlag = fb.ReadI32(ref pos);
                var compressedLength = fb.ReadI32(ref pos) - 1;
                var compressionAlgorithm = fb.ReadU8(ref pos);

                //Console.WriteLine(file.version + " " + compressionAlgorithm);
                if (compressionFlag == 2 && compressionAlgorithm == 2) {
                    compressed = true;
                    pos += 2; //skip zlib header
                    dataStream = DecompressZLIB(new MemoryStream(fb, (int)pos, compressedLength - 2));
                } else if (compressionFlag == 3 && compressionAlgorithm == 3) {
                    compressed = true;
                    dataStream = DecompressLZMA(new MemoryStream(fb, (int)pos, compressedLength));
                } else
                    throw new Exception("unknown compress method");
            }
            if(!compressed) 
                dataStream = new MemoryStream(fb, (int)pos,(int)segmentLength - 24);
            
            dataStream.FromJTFile(jtFile);
            dataStream.Position = 0;
            elements = new List<Element>();
            for(;dataStream.Position < dataStream.Length ; ) {
                var ele = new Element(this);
                ele.Instantiate();
                if (ObjectTypeIdentifiers.isEOE(ele.objectTypeID))
                    break;
                elements.Add(ele);
            }
            dataStream.RemoveJTFileBind();
            dataStream = null;

            //Debug.Log("Segment type(#b" +
            //          SegmentTypes.GetType(segmentType).Item1 + "#w) ID:" + segmentID + " begin:" + fs.Position +
            //          " len:" +
            //          segmentLength + " end:" + (fs.Position + segmentLength) + "| dataLen:" + dataStream.Length);


        }

        public Stream DecompressZLIB(Stream compressed) {
            var deflate = new DeflateStream(compressed, CompressionMode.Decompress);
            var decompressed = new MemoryStream();
            deflate.CopyTo(decompressed);
            deflate.Dispose();
            decompressed.Position = 0;
            return decompressed;
        }
        
        public Stream DecompressLZMA(Stream compressed) {
            var xzDecompressOptions = new XZDecompressOptions();
            xzDecompressOptions.BufferSize = 0;
            var xz = new XZStream(compressed, xzDecompressOptions);
            var decompressed = new MemoryStream();
            xz.CopyTo(decompressed);
            xz.Dispose();
            decompressed.Position = 0;
            return decompressed;
        }
    }
}
