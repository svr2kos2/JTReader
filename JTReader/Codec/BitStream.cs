using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLAT.JTReader {
    public class BitStream {
        private long _offset;
        private long _position;
        private long _length;
        private Stream _byteStream;

        public JTFile fromJTFile => _byteStream.FromJTFile();

        public long Length => _length;
        public long Position => _position - _offset;

        public BitStream(Stream stream, int bitLength = -1) {
            _byteStream = stream;
            _offset = stream.Position << 3;
            _position = _offset;
            _length = bitLength == -1 ? (stream.Length - stream.Position) << 3 : bitLength;
        }

        public BitStream(long position) {
            _position = position;
        }

        public int ReadI32(int bitLength) {
            var res = ReadU32(bitLength);
            res <<= (32 - bitLength);
            res >>= (32 - bitLength);
            return res;
        }
        public int ReadU32(int bitLength) {
            if (bitLength < 1)
                return 0;
            var res = 0;
            for (int i = 0; i < bitLength; ++i) {
                res <<= 1;
                var pos = _position + i;
                _byteStream.Position = pos / 8;
                var b = _byteStream.ReadU8();
                //read from high bit
                res |= (b & (1 << (int)(7 - (pos % 8)))) == 0 ? 0 : 1;
            }
            _position += bitLength;
            ApplyPositionToByteStream();
            return res;
        }
        public float ReadF32(int bitLength) {
            var bytes = BitConverter.GetBytes(ReadI32(bitLength));
            return BitConverter.ToSingle(bytes, 0);
        }

        private void ApplyPositionToByteStream() {
            var pos = _position / 8 + (_position % 8 == 0 ? 0 : 1);
            _byteStream.Position = pos;
        }

    }
}
