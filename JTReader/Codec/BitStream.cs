using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLAT.JTReader {
    public class BitStream {
        long position;
        long length;
        Stream byteStream;

        public long Length { get { return length; } }
        public long Position { get { return position; } }

        public BitStream(Stream stream, int bitLength = -1) {
            byteStream = stream;
            position = stream.Position << 3;
            length = bitLength == -1 ? stream.Length << 3 : bitLength;
        }

        public int ReadI32(int bitLength) {
            int res = 0;
            for (int i = 0; i < bitLength; ++i) {
                res <<= 1;
                var pos = position + i;
                byteStream.Position = pos / 8;
                var b = byteStream.ReadU8();
                res |= (b & (1 << (int)(7 - (pos % 8)))) == 0 ? 0 : 1;
            }
            position += bitLength;
            return res;
        }
        public uint ReadU32(int bitLength) {
            var bytes = BitConverter.GetBytes(ReadI32(bitLength));
            return BitConverter.ToUInt32(bytes, 0);
        }
        public float ReadF32(int bitLength) {
            var bytes = BitConverter.GetBytes(ReadI32(bitLength));
            return BitConverter.ToSingle(bytes, 0);
        }

        public void ApplyPositionToByteStream() {
            var pos = position / 8 + (position % 8 == 0 ? 0 : 1);
            byteStream.Position = pos;
        }

    }
}
