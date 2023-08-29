using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DLAT.JTReader {
    public static class StreamReader {
        private static ConcurrentDictionary<Stream, JTFile> _fromJTFile = new ConcurrentDictionary<Stream, JTFile>();
        public static JTFile FromJTFile(this Stream stream, JTFile setFile = null) {
            if (_fromJTFile.ContainsKey(stream)) {
                if (setFile == null)
                    return _fromJTFile[stream];
                else {
                    _fromJTFile[stream] = setFile;
                    return setFile;
                }
            }
            if (setFile == null)
                throw new Exception("JTFile order not set");
            _fromJTFile.TryAdd(stream, setFile);
            return setFile;
        }

        public static void RemoveJTFileBind(this Stream stream) {
            if (_fromJTFile.ContainsKey(stream))
                _fromJTFile.TryRemove(stream, out var res);
        }
        
        public static byte[] ReadBytes(this Stream stream, int count, int padEnd = 0) {
            var buffer = new byte[count + padEnd];
            var read = stream.Read(buffer, 0, count);
            if (read < count)
                throw new Exception("Over read");
            return buffer;
        }
        public static byte[] ReadBytes(this Stream stream, long pos, int count, int padEnd = 0) {
            stream.Position = pos;
            return ReadBytes(stream, count);
        }
        public static byte ReadU8(this Stream stream) {
            return ReadBytes(stream, 1)[0];
        }
        public static short ReadI16(this Stream stream) {
            return BitConverter.ToInt16(ReadBytes(stream, 2), 0);
        }
        public static ushort ReadU16(this Stream stream) {
            return BitConverter.ToUInt16(ReadBytes(stream, 2), 0);
        }
        public static int ReadI32(this Stream stream) {
            return BitConverter.ToInt32(ReadBytes(stream, 4), 0);
        }
        public static uint ReadU32(this Stream stream) {
            return BitConverter.ToUInt32(ReadBytes(stream, 4), 0);
        }
        public static long ReadI64(this Stream stream) {
            return BitConverter.ToInt64(ReadBytes(stream, 8), 0);
        }
        public static ulong ReadU64(this Stream stream) {
            return BitConverter.ToUInt64(ReadBytes(stream, 8), 0);
        }
        public static GUID ReadGUID(this Stream stream) {
            return new GUID(ReadBytes(stream, 16));
        }
        public static float ReadF32(this Stream stream) {
            return BitConverter.ToSingle(ReadBytes(stream, 4), 0);
        }
        public static double ReadF64(this Stream stream) {
            return BitConverter.ToDouble(ReadBytes(stream, 8), 0);
        }
        public static string ReadString(this Stream stream) {
            return ReadString(stream, stream.ReadI32());
        }
        public static string ReadString(this Stream stream, int len) {
            return Encoding.UTF8.GetString(ReadBytes(stream, len));
        }
        public static string ReadMbString(this Stream stream) {
            var len = stream.ReadI32();
            return Encoding.Unicode.GetString(ReadBytes(stream, len * 2));
        }
        public static RGBA ReadRGBA(this Stream stream) {
            return new RGBA(stream);
        }
        public static List<int> ReadVecI32(this Stream data) {
            var count = data.ReadI32();
            var res = new List<int>(count);
            for (int i = 0; i < count; ++i)
                res.Add(data.ReadI32());
            return res;
        }
        public static List<uint> ReadVecU32(this Stream data) {
            var count = data.ReadI32();
            var res = new List<uint>(count);
            for (int i = 0; i < count; ++i)
                res.Add(data.ReadU32());
            return res;
        }
        public static List<float> ReadVecF32(this Stream data) {
            var count = data.ReadI32();
            var res = new List<float>(count);
            for (int i = 0; i < count; ++i)
                res.Add(data.ReadF32());
            return res;
        }

        public static byte ReadVersionNumber(this Stream data) {
            var jt = data.FromJTFile();
            return jt.majorVersion > 9 ? data.ReadU8() : (byte)data.ReadI16();
        }
        
    }
}
