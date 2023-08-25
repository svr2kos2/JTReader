using Joveler.Compression.XZ;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace DLAT.JTReader {
    public class JTFile {
        string _version;
        public string Version {
            get {
                return _version;
            }
            set {
                var sp = value.Split(' ');
                _version = sp[1];
                majorVersion = int.Parse(_version.Split('.')[0]);
                minorVersion = int.Parse(_version.Split('.')[1]);
            }
        }
        public int majorVersion;
        public int minorVersion;
        public readonly int byteOrder;
        
        public Stream fileStream;
        public List<DataSegment> segments;
        public DataSegment LSGSegment;

        static bool isXZInited = false;

        public JTFile(string filePath) {

            if(!isXZInited) {
                InitNativeLibrary();
                isXZInited = true;
            }

            fileStream = new FileStream(filePath, FileMode.Open);
            Version = fileStream.ReadString(80);

            Console.WriteLine("JT File Version " + Version);

            byteOrder = fileStream.ReadU8();
            fileStream.FromJTFile(this);
            if (byteOrder == 1)
                throw new Exception("This file saved as big endian. Doesn't support yet.");
            var emptyField = fileStream.ReadI32();
            ulong TOCOffset = majorVersion > 9 ? fileStream.ReadU64() : (ulong)fileStream.ReadI32();
            var LSGSegmentID = new GUID(fileStream);

            fileStream.Position = (long)TOCOffset;
            var entryCount = fileStream.ReadI32();
            Debug.Log("Segment Count:" + entryCount);
            segments = new List<DataSegment>();
            for (int i = 0; i < entryCount; ++i) {
                var seg = new DataSegment(this);
                if(seg.segmentID == LSGSegmentID)
                    LSGSegment = seg;
                segments.Add(seg);
            }
            fileStream.Close();
        }


        public static void InitNativeLibrary() {
            string arch = null;
            switch (RuntimeInformation.ProcessArchitecture) {
                case Architecture.X86:
                    arch = "x86";
                    break;
                case Architecture.X64:
                    arch = "x64";
                    break;
                case Architecture.Arm:
                    arch = "armhf";
                    break;
                case Architecture.Arm64:
                    arch = "arm64";
                    break;
            }
            string libPath = Path.Combine(arch, "liblzma.dll");

            if (!File.Exists(libPath))
                throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");

            XZInit.GlobalInit(libPath);
        }

    }
}
