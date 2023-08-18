using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace DLAT.JTReader {
    public class VertexBasedShapeCompressedRepData {
        public const byte BINDING_NONE = 0;
        /** Binding: Per vertex */
        public const byte BINDING_PER_VERTEX = 1;
        /** Binding: Per facet */
        public const byte BINDING_PER_FACET = 2;
        /** Binding: Per primitive */
        public const byte BINDING_PER_PRIMITIVE = 3;
        public short versionNumber;
        public byte normalBinding;
        public byte textureCoordBinding;
        public byte colorBinding;
        public QuantizationParameters parameters;

        public List<int> indices;
        public LosslessCompressedRawVertexData losslessCompressedRawVertex;
        public LossyQuantizedRawVertexData lossyQuantizedRawVertex;
        
        public VertexBasedShapeCompressedRepData(Stream data) {
            versionNumber = data.ReadI16();
            normalBinding = data.ReadU8();
            textureCoordBinding = data.ReadU8();
            colorBinding = data.ReadU8();
            parameters = new QuantizationParameters(data);
            indices = Int32CDP.ReadVecI32(data, PredictorType.PredStride1);
            if (parameters.bitsPerVertex == 0)
                losslessCompressedRawVertex = new LosslessCompressedRawVertexData(data, textureCoordBinding, colorBinding, normalBinding);
            else
                lossyQuantizedRawVertex = new LossyQuantizedRawVertexData(data, textureCoordBinding, colorBinding, normalBinding);
        }

        public List<double> GetVertices() {
            if (losslessCompressedRawVertex != null)
                return losslessCompressedRawVertex.vertices;
            else
                return lossyQuantizedRawVertex.GetVertices();
        }
        public List<int> GetIndices() {
            return indices;
        }
        public List<float> GetColors() {
            if (losslessCompressedRawVertex != null)
                return losslessCompressedRawVertex.colors;
            else
                return lossyQuantizedRawVertex.GetColors();
        }

    }
    public class LosslessCompressedRawVertexData {
        public List<float> textureCoordinates;
        public List<float> colors;
        public List<double> normals;
        public List<double> vertices;
        public LosslessCompressedRawVertexData(Stream data,int textureCoordBinding, int colorBinding, int normalBinding) {
            int uncompressedDataSize = data.ReadI32();
            int compressedDataSize = data.ReadI32();
            float[] rawVertexData = null;

            //uncompressed raw data
            if(compressedDataSize < 0) {
                compressedDataSize *= -1;
                rawVertexData = new float[compressedDataSize / 4];
                for (int i = 0; i < rawVertexData.Length; i++) {
                    rawVertexData[i] = data.ReadF32();
                }
                //zlib compressed raw data
            } else if(compressedDataSize > 0) {
                var zlibHeader = data.ReadBytes(2);
                var compressedBytes = new MemoryStream(data.ReadBytes(compressedDataSize));
                var uncompressedStream = new MemoryStream();
                var uncompressedDelate = new DeflateStream(compressedBytes, CompressionMode.Decompress);
                uncompressedDelate.CopyTo(uncompressedStream);
                uncompressedStream.Position = 0;
                if (uncompressedStream.Length != uncompressedDataSize) {
                    throw new Exception("ZLIB decompression seems to be failed! Expected length: " + uncompressedDataSize + " -> resulting length: " + uncompressedStream.Length);
                }

                rawVertexData = new float[uncompressedStream.Length / 4];
                for (int i = 0; i < rawVertexData.Length; i++) {
                    rawVertexData[i] = uncompressedStream.ReadF32();
                }
            } else {
                throw new Exception("Found invalid compressed data size: " + compressedDataSize);
            }

            // Create derived lists
            textureCoordinates = new List<float>();
            colors = new List<float>();
            normals = new List<double>();
            vertices = new List<double>();

            bool readTextureCoordinate = textureCoordBinding == VertexBasedShapeCompressedRepData.BINDING_PER_VERTEX;
            bool readColor = colorBinding == VertexBasedShapeCompressedRepData.BINDING_PER_VERTEX;
            bool readNormal = normalBinding == VertexBasedShapeCompressedRepData.BINDING_PER_VERTEX;

            for (int i = 0; i < rawVertexData.Length;) {
                if (readTextureCoordinate) {
                    // Read U, V
                    textureCoordinates.Add(rawVertexData[i++]);
                    textureCoordinates.Add(rawVertexData[i++]);
                }

                if (readColor) {
                    // Read R, G, B
                    colors.Add(rawVertexData[i++]);
                    colors.Add(rawVertexData[i++]);
                    colors.Add(rawVertexData[i++]);
                }

                if (readNormal) {
                    // Read NX, NY, NZ
                    normals.Add(rawVertexData[i++]);
                    normals.Add(rawVertexData[i++]);
                    normals.Add(rawVertexData[i++]);
                }

                // Read X, Y, Z
                vertices.Add(rawVertexData[i++]);
                vertices.Add(rawVertexData[i++]);
                vertices.Add(rawVertexData[i++]);
            }
        }
    }
    public class LossyQuantizedRawVertexData {
        public QuantizedVertexCoordArray quantizedVertexCoordArray;
        public QuantizedVertexNormalArray quantizedNormalArray;
        public QuantizedVertexTextureCoordArray quantizedTextureCoordArray;
        public QuantizedVertexColorArray quantizedVertexColorArray;
        public List<int> vertexDataIndices;
        public LossyQuantizedRawVertexData(Stream data, int textureCoordBinding,int colorBinding,int normalBinding) {
            quantizedVertexCoordArray = new QuantizedVertexCoordArray(data);
            if(normalBinding !=0)
                quantizedNormalArray = new QuantizedVertexNormalArray(data);
            if(textureCoordBinding != 0)
                quantizedTextureCoordArray = new QuantizedVertexTextureCoordArray(data);
            if(colorBinding != 0)
                quantizedVertexColorArray = new QuantizedVertexColorArray(data);
            vertexDataIndices = Int32CDP.ReadVecU32(data, PredictorType.PredStripIndex);  
        }
        public List<double> GetVertices() {
            List<double> unsortedVertices = quantizedVertexCoordArray.GetVertices();
            List<double> sortedVertices = new List<double>();
            foreach (int vertexIndex in vertexDataIndices) {
                sortedVertices.Add(unsortedVertices[vertexIndex * 3]);
                sortedVertices.Add(unsortedVertices[(vertexIndex * 3) + 1]);
                sortedVertices.Add(unsortedVertices[(vertexIndex * 3) + 2]);
            }
            return sortedVertices;
        }
        public List<float> GetColors() {
            List<float> res = new List<float>();
            foreach (var v in quantizedVertexColorArray.GetColors())
                res.Add((float)v);
            return res;
        }
    }
}