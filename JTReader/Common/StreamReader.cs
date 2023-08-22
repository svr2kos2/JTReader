using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DLAT.JTReader {
    public static class StreamReader {
        private static Dictionary<Stream,int> byteOrders = new Dictionary<Stream, int>();
        public static int ByteOrder(this Stream stream, int? setOrder = null) {
            if (byteOrders.ContainsKey(stream)) {
                if (setOrder == null)
                    return byteOrders[stream];
                else {
                    byteOrders[stream] = setOrder.Value;
                    return setOrder.Value;
                }
            }
            if (setOrder == null)
                throw new Exception("Byte order not set");
            byteOrders.Add(stream, setOrder.Value);
            return setOrder.Value;
        }
        public static byte[] ReadBytes(this Stream stream, int count, int padEnd = 0) {
            var buffer = new byte[count + padEnd];
            stream.Read(buffer, 0, count);
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
    }
}
