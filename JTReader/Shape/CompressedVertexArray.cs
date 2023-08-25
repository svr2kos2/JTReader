using System;
using System.Collections.Generic;
using System.IO;

namespace DLAT.JTReader {
    public class CompressedVertexCoordinateArray {
        public List<double> vertexCoordinates;

        public CompressedVertexCoordinateArray(Stream data) {
            if (data.FromJTFile().majorVersion == 9)
                ReadV9(data);
            else
                ReadV10(data);

        }

        void ReadV9(Stream data) {
            int uniqueVertexCount = data.ReadI32();
            int numberComponents = data.ReadU8();
            PointQuantizerData pointQuantizerData = new PointQuantizerData(data);

            List<List<int>> vertexCoordExponentLists = new List<List<int>>();
            List<List<int>> vertexCoordMantissaeLists = new List<List<int>>();
            List<List<int>> vertexCoordCodeLists = new List<List<int>>();
            vertexCoordinates = new List<double>();
            int numberOfBits = pointQuantizerData.numberOfBits;
            if (numberOfBits == 0) {
                for (int i = 0; i < numberComponents; i++) {
                    List<int> exponents = Int32CDP.ReadVecI32(data, PredictorType.PredLag1);
                    List<int> mantissae = Int32CDP.ReadVecI32(data, PredictorType.PredLag1);
                    List<int> codeData = new List<int>();
                    for (int j = 0; j < exponents.Count; j++)
                        codeData.Add((exponents[j] << 23) | mantissae[j]);
                    vertexCoordExponentLists.Add(exponents);
                    vertexCoordMantissaeLists.Add(mantissae);
                    vertexCoordCodeLists.Add(codeData);
                }

                List<int> xCodeData = vertexCoordCodeLists[0];
                List<int> yCodeData = vertexCoordCodeLists[1];
                List<int> zCodeData = vertexCoordCodeLists[2];
                for (int i = 0; i < xCodeData.Count; i++) {
                    vertexCoordinates.Add(BitConverter.ToSingle(BitConverter.GetBytes(xCodeData[i]), 0));
                    vertexCoordinates.Add(BitConverter.ToSingle(BitConverter.GetBytes(yCodeData[i]), 0));
                    vertexCoordinates.Add(BitConverter.ToSingle(BitConverter.GetBytes(zCodeData[i]), 0));
                }
            }
            else if (numberOfBits > 0) {
                vertexCoordCodeLists.Add(Int32CDP.ReadVecU32(data, PredictorType.PredLag1));
                vertexCoordCodeLists.Add(Int32CDP.ReadVecU32(data, PredictorType.PredLag1));
                vertexCoordCodeLists.Add(Int32CDP.ReadVecU32(data, PredictorType.PredLag1));
                
                List<Double> xValues =
                    CODEC.Dequantize(vertexCoordCodeLists[0], pointQuantizerData.xRange, numberOfBits);
                List<Double> yValues =
                    CODEC.Dequantize(vertexCoordCodeLists[1], pointQuantizerData.yRange, numberOfBits);
                List<Double> zValues =
                    CODEC.Dequantize(vertexCoordCodeLists[2], pointQuantizerData.zRange, numberOfBits);
                for (int i = 0; i < xValues.Count; i++) {
                    vertexCoordinates.Add(xValues[i]);
                    vertexCoordinates.Add(yValues[i]);
                    vertexCoordinates.Add(zValues[i]);
                }
            }
            else {
                throw new Exception("ERROR: Negative number of quantized bits: " + numberOfBits);
            }
            
            long readHash = data.ReadU32();
        }

        void ReadV10(Stream data) {
            int uniqueVertexCount = data.ReadI32();
            int numberComponents = data.ReadU8();
            PointQuantizerData pointQuantizerData = new PointQuantizerData(data);
            
            vertexCoordinates = new List<double>();
            int numberOfBits = pointQuantizerData.numberOfBits;
            if (numberOfBits == 0) {
                var binaryVertexCoords = new List<int>[numberComponents];
                for (int i = 0; i < numberComponents; i++) {
                    binaryVertexCoords[i] = Int32CDP.ReadVecU32(data, PredictorType.PredLag1);
                }

            }
            else if (numberOfBits > 0) {
                var vertexCoordCodeLists = new List<int>[3];
                vertexCoordCodeLists[0] = Int32CDP.ReadVecU32(data, PredictorType.PredLag1);
                vertexCoordCodeLists[1] = Int32CDP.ReadVecU32(data, PredictorType.PredLag1);
                vertexCoordCodeLists[2] = Int32CDP.ReadVecU32(data, PredictorType.PredLag1);
                
                List<Double> xValues =
                    CODEC.Dequantize(vertexCoordCodeLists[0], pointQuantizerData.xRange, numberOfBits);
                List<Double> yValues =
                    CODEC.Dequantize(vertexCoordCodeLists[1], pointQuantizerData.yRange, numberOfBits);
                List<Double> zValues =
                    CODEC.Dequantize(vertexCoordCodeLists[2], pointQuantizerData.zRange, numberOfBits);
                for (int i = 0; i < xValues.Count; i++) {
                    vertexCoordinates.Add(xValues[i]);
                    vertexCoordinates.Add(yValues[i]);
                    vertexCoordinates.Add(zValues[i]);
                }
            }
            else {
                throw new Exception("ERROR: Negative number of quantized bits: " + numberOfBits);
            }
            
            long readHash = data.ReadU32();
        }
    }
    public class CompressedVertexNormalArray {
        public List<double> normalCoordinates;

        public CompressedVertexNormalArray(Stream data) {
            if(data.FromJTFile().majorVersion == 9)
                ReadV9(data);
            else {
                ReadV10(data);
            }
        }

        public void ReadV9(Stream data) {
            var normalCount = data.ReadI32();
            var numberComponents = data.ReadU8();
            var quantizationBits = data.ReadU8();

            var vertexNormalExponentsLists = new List<List<int>>();
            var vertexNormalMantissaeLists = new List<List<int>>();
            var sextantCodes = new List<int>();
            var octantCodes = new List<int>();
            var thetaCodes = new List<int>();
            var psiCodes = new List<int>();
            normalCoordinates = new List<double>();
            var normalVectorLists = new List<List<int>>();

            if (quantizationBits == 0) {
                for (int i = 0; i < numberComponents; i++) {
                    List<int> exponents = Int32CDP.ReadVecI32(data, PredictorType.PredNull);
                    List<int> mantissae = Int32CDP.ReadVecI32(data, PredictorType.PredNull);

                    List<int> normalVectorData = new List<int>();
                    for (int j = 0; j < exponents.Count; j++) {
                        normalVectorData.Add((exponents[j] << 23) | mantissae[j]);
                    }

                    normalVectorLists.Add(normalVectorData);
                    vertexNormalExponentsLists.Add(exponents);
                    vertexNormalMantissaeLists.Add(mantissae);
                }

                List<int> xCodeData = normalVectorLists[0];
                List<int> yCodeData = normalVectorLists[1];
                List<int> zCodeData = normalVectorLists[2];
                for (int i = 0; i < xCodeData.Count; i++) {
                    normalCoordinates.Add(BitConverter.ToSingle(BitConverter.GetBytes(xCodeData[i]), 0));
                    normalCoordinates.Add(BitConverter.ToSingle(BitConverter.GetBytes(yCodeData[i]), 0));
                    normalCoordinates.Add(BitConverter.ToSingle(BitConverter.GetBytes(zCodeData[i]), 0));
                }
            }
            else if (quantizationBits > 0) {
                sextantCodes = Int32CDP.ReadVecI32(data, PredictorType.PredNull);
                octantCodes  = Int32CDP.ReadVecI32(data, PredictorType.PredNull);
                thetaCodes   = Int32CDP.ReadVecI32(data, PredictorType.PredNull);
                psiCodes     = Int32CDP.ReadVecI32(data, PredictorType.PredNull);

                var deeringCodec = new CODEC.DeeringNormalCodec(quantizationBits);
                for (int i = 0; i < psiCodes.Count; i++) {
                    var normal =
                        deeringCodec.convertCodeToVec(sextantCodes[i], octantCodes[i], thetaCodes[i], psiCodes[i]);
                    normalCoordinates.Add(normal.x);
                    normalCoordinates.Add(normal.y);
                    normalCoordinates.Add(normal.z);
                }
            }
            else {
                throw new Exception("ERROR: Negative number of quantized bits: " + quantizationBits);
            }

            var hash = data.ReadU32();
        }

        public void ReadV10(Stream data) {
            var normalCount = data.ReadI32();
            var numberComponents = data.ReadU8();
            var quantizationBits = data.ReadU8();

            if (quantizationBits == 0) {
                var binaryVertexNormals = new List<int>[numberComponents];
                for (int i = 0; i < numberComponents; ++i)
                    binaryVertexNormals[i] = Int32CDP.ReadVecU32(data, PredictorType.PredNull);
            }
            else {
                var deeringNormalCodes = Int32CDP.ReadVecU32(data, PredictorType.PredNull);
            }

            var hash = data.ReadU32();
        }
        
    }
    public class CompressedVertexColorArray {
        public List<double> colorValues;

        public CompressedVertexColorArray(Stream data) {
            var normalCount = data.ReadI32();
            var numberComponents = data.ReadU8();
            var quantizationBits = data.ReadU8();

            var vertexColorExponentsLists = new List<List<int>>();
            var vertexColorMantissaeLists = new List<List<int>>();
            var vertexColorCodeLists = new List<List<int>>();
            var hueRedCodes = new List<int>();
            var satGreenCodes = new List<int>();
            var valueBlueCodes = new List<int>();
            var alphaCodes = new List<int>();
            colorValues = new List<double>();

            if (quantizationBits == 0) {
                for (int i = 0; i < numberComponents; i++) {
                    List<int> exponents = Int32CDP.ReadVecI32(data, PredictorType.PredLag1);
                    List<int> mantissae = Int32CDP.ReadVecI32(data, PredictorType.PredLag1);

                    List<int> codeData = new List<int>();
                    for (int j = 0; j < exponents.Count; j++)
                        codeData.Add((exponents[j] << 23) | mantissae[j]);


                    vertexColorExponentsLists.Add(exponents);
                    vertexColorMantissaeLists.Add(mantissae);
                    vertexColorCodeLists.Add(codeData);
                }

                List<int> redCodeData = vertexColorCodeLists[0];
                List<int> greenCodeData = vertexColorCodeLists[1];
                List<int> blueCodeData = vertexColorCodeLists[2];
                for (int i = 0; i < redCodeData.Count; i++) {
                    colorValues.Add(BitConverter.ToSingle(BitConverter.GetBytes(redCodeData[i]), 0));
                    colorValues.Add(BitConverter.ToSingle(BitConverter.GetBytes(greenCodeData[i]), 0));
                    colorValues.Add(BitConverter.ToSingle(BitConverter.GetBytes(blueCodeData[i]), 0));
                }
            }
            else {
                var colorQuantizerData = new ColorQuantizerData(data);

                hueRedCodes    = Int32CDP.ReadVecI32(data, PredictorType.PredLag1);
                satGreenCodes  = Int32CDP.ReadVecI32(data, PredictorType.PredLag1);
                valueBlueCodes = Int32CDP.ReadVecI32(data, PredictorType.PredLag1);
                alphaCodes     = Int32CDP.ReadVecI32(data, PredictorType.PredLag1);

                List<Double> redValues = CODEC.Dequantize(hueRedCodes, colorQuantizerData.rRange, quantizationBits);
                List<Double> greenValues = CODEC.Dequantize(satGreenCodes, colorQuantizerData.gRange, quantizationBits);
                List<Double> blueValues = CODEC.Dequantize(valueBlueCodes, colorQuantizerData.bRange, quantizationBits);
                for (int i = 0; i < redValues.Count; i++) {
                    colorValues.Add(redValues[i]);
                    colorValues.Add(greenValues[i]);
                    colorValues.Add(blueValues[i]);
                }
            }

            long readHash = data.ReadU32();
        }
    }
    public class CompressedVertexTextureCoordinateArray {
        public List<double> textureCoordinates;

        public CompressedVertexTextureCoordinateArray(Stream data) {
            int textureCoordCount = data.ReadI32();
		    int numberComponents  = data.ReadU8();
		    int quantizationBits  = data.ReadU8();

		    List<List<int>> vertexTextureCoordExponentLists = new List<List<int>>();
		    List<List<int>> vertexTextureCoordMantissaeLists = new List<List<int>>();
		    List<List<int>> vertexTextureCodeLists = new List<List<int>>();
		    List<List<int>> textureCoordCodesLists = new List<List<int>>();
		    TextureQuantizerData textureQuantizerData = null;
		    List<Double> textureCoordinates = new List<Double>();

		    if(quantizationBits == 0){
			    for(int i = 0; i < numberComponents; i++){
				    List<int> exponents = Int32CDP.ReadVecI32(data, PredictorType.PredNull);
				    List<int> mantissae = Int32CDP.ReadVecI32(data, PredictorType.PredNull);

				    List<int> codeData = new List<int>();
				    for(int j = 0; j < exponents.Count; j++){
					    codeData.Add((exponents[j] << 23) | mantissae[j]);
				    }

				    vertexTextureCoordExponentLists.Add(exponents);
				    vertexTextureCoordMantissaeLists.Add(mantissae);
				    vertexTextureCodeLists.Add(codeData);
			    }

			    List<int> uCodeData = vertexTextureCodeLists[0];
			    List<int> vCodeData = vertexTextureCodeLists[1];
			    for(int i = 0; i < uCodeData.Count; i++) {
                    textureCoordinates.Add(BitConverter.ToSingle(BitConverter.GetBytes(uCodeData[i]), 0));
                    textureCoordinates.Add(BitConverter.ToSingle(BitConverter.GetBytes(vCodeData[i]), 0));
                }

		    } else {
			    textureQuantizerData = new TextureQuantizerData(data, numberComponents);

			    for(int i = 0; i < numberComponents; i++){
				    textureCoordCodesLists.Add(Int32CDP.ReadVecU32(data, PredictorType.PredLag1));
			    }

			    List<Double> uValues = CODEC.Dequantize(textureCoordCodesLists[0], textureQuantizerData.uRange, quantizationBits);
			    List<Double> vValues = CODEC.Dequantize(textureCoordCodesLists[1], textureQuantizerData.vRange, quantizationBits);
			    for(int i = 0; i < uValues.Count; i++){
				    textureCoordinates.Add(uValues[i]);
				    textureCoordinates.Add(vValues[i]);
			    }

			    // Ignore the hash value
			    var hash = data.ReadU32();

		    }
        }
    }
    public class CompressedVertexFlagArray {
        public List<int> vertexFlags;

        public CompressedVertexFlagArray(Stream data) {
            var count = data.ReadI32();
            vertexFlags = Int32CDP.ReadVecU32(data, PredictorType.PredNull);
        }
    }
    
}