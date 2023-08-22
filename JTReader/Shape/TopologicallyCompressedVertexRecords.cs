using System.IO;

namespace DLAT.JTReader {
    public class TopologicallyCompressedVertexRecords {
        public CompressedVertexCoordinateArray compressedVertexCoordinateArray;
        public CompressedVertexNormalArray compressedVertexNormalArray;
        public CompressedVertexColorArray compressedVertexColorArray;

        public TopologicallyCompressedVertexRecords(Stream data) {
            ulong vertexBindings = data.ReadU64();
            var quantizationParameters = new QuantizationParameters(data);
            var numberOfTopologicalVertices = data.ReadI32();

            if (numberOfTopologicalVertices <= 0)
                return;

            var numberOfVertexAttributes = data.ReadI32();

            //CompressedVertexCoordinateArray compressedVertexCoordinateArray = null;
            if ((vertexBindings & 0x07) != 0) { // Check for bits 1-3
                compressedVertexCoordinateArray = new CompressedVertexCoordinateArray(data);
            }

            //CompressedVertexNormalArray compressedVertexNormalArray = null;
            if ((vertexBindings & 0x08) != 0) { // Check for bit 4
                compressedVertexNormalArray = new CompressedVertexNormalArray(data);
            }

            //CompressedVertexColorArray compressedVertexColorArray = null;
            if ((vertexBindings & 0x30) != 0) { // Check for bits 5-6
                compressedVertexColorArray = new CompressedVertexColorArray(data);
            }

            CompressedVertexTextureCoordinateArray[] compressedVertexTextureCoordinateArrays =
                new CompressedVertexTextureCoordinateArray[8];
            if ((vertexBindings & 0xf00) != 0) { // Check for bits 9-12
                compressedVertexTextureCoordinateArrays[0] = new CompressedVertexTextureCoordinateArray(data);
            }

            if ((vertexBindings & 0xf000) != 0) { // Check for bits 13-16
                compressedVertexTextureCoordinateArrays[1] = new CompressedVertexTextureCoordinateArray(data);
            }

            if ((vertexBindings & 0xf0000) != 0) { // Check for bits 17-20
                compressedVertexTextureCoordinateArrays[2] = new CompressedVertexTextureCoordinateArray(data);
            }

            if ((vertexBindings & 0xf00000) != 0) { // Check for bits 21-24
                compressedVertexTextureCoordinateArrays[3] = new CompressedVertexTextureCoordinateArray(data);
            }

            if ((vertexBindings & 0xf000000) != 0) { // Check for bits 25-28
                compressedVertexTextureCoordinateArrays[4] = new CompressedVertexTextureCoordinateArray(data);
            }

            if ((vertexBindings & 0xf0000000) != 0) { // Check for bits 29-32
                compressedVertexTextureCoordinateArrays[5] = new CompressedVertexTextureCoordinateArray(data);
            }

            if ((vertexBindings & 0xf00000000ul) != 0) { // Check for bits 33-36
                compressedVertexTextureCoordinateArrays[6] = new CompressedVertexTextureCoordinateArray(data);
            }

            if ((vertexBindings & 0xf000000000ul) != 0) { // Check for bits 37-40
                compressedVertexTextureCoordinateArrays[7] = new CompressedVertexTextureCoordinateArray(data);
            }

            CompressedVertexFlagArray compressedVertexFlagArray = null;
            if ((vertexBindings & 0x40) != 0) { // Check for bit 7
                compressedVertexFlagArray = new CompressedVertexFlagArray(data);
            }
        }
    }
}