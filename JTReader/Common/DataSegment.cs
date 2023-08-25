using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Runtime.InteropServices.ComTypes;
using Joveler.Compression.XZ;
using System.Diagnostics;

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

        public void InitializeElements() {
            dataStream.Position = 0;

            elements = new List<Element>();
            for(;dataStream.Position < dataStream.Length ; ) {
                var ele = new Element(this);
                if (ObjectTypeIdentifiers.isEOE(ele.objectTypeID))
                    break;
                elements.Add(ele);
            }

        }

        public static HashSet<(string,int)> versionCompressAlg = new HashSet<(string,int)>();

        public DataSegment(JTFile jtFile) {
            file = jtFile;
            var fs = file.fileStream;
            //----TOC Entry----
            segmentID = fs.ReadGUID();
            var segmentOffset = majorVersion > 9 ? fs.ReadI64() : fs.ReadI32();
            segmentLength = fs.ReadU32();
            var segmentAttribute = fs.ReadBytes(4);
            segmentType = segmentAttribute[3];
            //----TOC Entry----

            long TOCEndPosition = fs.Position;

            //----Segment Header----
            fs.Position = segmentOffset;
            if (segmentID != fs.ReadGUID())
                throw new Exception("TOC Entry Segment ID doesn't equal to Segment Header Segment ID");
            if (segmentType != fs.ReadI32())
                throw new Exception("TOC Entry Segment Type doesn't equal to Segment Header Segment Type");
            if (segmentLength != fs.ReadI32())
                throw new Exception("TOC Entry Segment Length doesn't equal to Segment Header Segment Length");
            //----Segment Header----

            var compressed = false;
            var supportCompress = SegmentTypes.GetType(segmentType).Item2;
            if(supportCompress) {
                var compressionFlag = fs.ReadI32();
                var compressedLength = fs.ReadI32() - 1;
                var compressionAlgorithm = fs.ReadU8();

                versionCompressAlg.Add((file.Version, compressionAlgorithm));
                //Console.WriteLine(file.version + " " + compressionAlgorithm);
                if (compressionFlag == 2 && compressionAlgorithm == 2) {
                    compressed = true;
                    var zlibHeader = fs.ReadBytes(2);
                    dataStream = DecompressZLIB(new MemoryStream(fs.ReadBytes(compressedLength - 2)));
                }
                else if(compressionFlag == 3 && compressionAlgorithm == 3) {
                    compressed = true;
                    dataStream = DecompressLZMA(new MemoryStream(fs.ReadBytes(compressedLength)));
                }
            }
            if(!compressed) 
                dataStream = new MemoryStream(fs.ReadBytes((int)segmentLength - 24));

            dataStream.FromJTFile(jtFile);
            fs.Position = TOCEndPosition;

            Debug.Log("Segment type(#b" +
                      SegmentTypes.GetType(segmentType).Item1 + "#w) ID:" + segmentID + " begin:" + fs.Position +
                      " len:" +
                      segmentLength + " end:" + (fs.Position + segmentLength) + "| dataLen:" + dataStream.Length);

            InitializeElements();

        }

        public Stream DecompressZLIB(Stream compressed) {
            var deflate = new DeflateStream(compressed, CompressionMode.Decompress);
            var decompressed = new MemoryStream();
            deflate.CopyTo(decompressed);
            decompressed.Position = 0;
            return decompressed;
        }

        public Stream DecompressLZMA(Stream compressed) {
            var xz = new XZStream(compressed, new XZDecompressOptions());
            var decompressed = new MemoryStream();
            xz.CopyTo(decompressed);
            decompressed.Position = 0;
            return decompressed;
        }
    }
}
