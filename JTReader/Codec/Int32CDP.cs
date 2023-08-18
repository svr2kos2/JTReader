﻿using DLAT.JTReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLAT.JTReader {
    public enum PredictorType {
        PredLag1 = 0,
        PredLag2 = 1,
        PredStride1 = 2,
        PredStride2 = 3,
        PredStripIndex = 4,
        PredRamp = 5,
        PredXor1 = 6,
        PredXor2 = 7,
        PredNull = 8
    }
    public class Int32CDP {
        public const int CODECTYPE_NULL = 0;
        public const int CODECTYPE_BITLENGTH = 1;
        public const int CODECTYPE_HUFFMAN = 2;
        public const int CODECTYPE_ARITHMETIC = 3;

        

        public static int[] DecodeBytes(Stream data) {
            byte codecType = data.ReadU8();
            int[] decodedSymbols;

            if (codecType == CODECTYPE_NULL) {
                var count = data.ReadI32();
                decodedSymbols = new int[count];
                for (var i = 0; i < count; ++i) {
                    decodedSymbols[i] = data.ReadI32();
                }
                return decodedSymbols;
            }


            Int32ProbabilityContexts int32ProbabilityContexts = null;
            int outOfBandValueCount;
            int codeTextLength;
            int valueElementCount;
            int[] outOfBandValues = null;
            int symbolCount;

            if (codecType == CODECTYPE_HUFFMAN || codecType == CODECTYPE_ARITHMETIC) {
                int32ProbabilityContexts = new Int32ProbabilityContexts(data);
                outOfBandValueCount = data.ReadI32();
                if (outOfBandValueCount > 0)
                    outOfBandValues = DecodeBytes(data);
            }

            codeTextLength = data.ReadI32();
            valueElementCount = data.ReadI32();
            symbolCount = valueElementCount;
            if (int32ProbabilityContexts != null && int32ProbabilityContexts.probabilityContextTableCount > 1)
                symbolCount = data.ReadI32();

            int intsToRead = data.ReadI32();
            byte[] codeText = new byte[intsToRead * 4];
            for (int i = 0; i < intsToRead; i++) {
                byte[] buffer = data.ReadBytes(4);
                if(data.ByteOrder() == 0){
                    codeText[i * 4] = buffer[3];
                    codeText[(i * 4) + 1] = buffer[2];
                    codeText[(i * 4) + 2] = buffer[1];
                    codeText[(i * 4) + 3] = buffer[0];
                } else {
                    codeText[i * 4] = buffer[0];
                    codeText[(i * 4) + 1] = buffer[1];
                    codeText[(i * 4) + 2] = buffer[2];
                    codeText[(i * 4) + 3] = buffer[3];
                }
            }


            CodecDriver codecDriver = new CodecDriver {
                codeTextBytes = codeText,
                codeTextLengthInBits = codeTextLength,
                valueElementCount = valueElementCount,
                symbolCount = symbolCount,
                int32ProbabilityContexts = int32ProbabilityContexts,
                bitSteam = new BitStream(new MemoryStream(codeText), codeTextLength),
                bitsRead = 0,
                outOfBandValues = outOfBandValues,
            };

            if(codecType == CODECTYPE_BITLENGTH)
                decodedSymbols = BitlengthDecoder.Decode(codecDriver);
            else if(codecType == CODECTYPE_HUFFMAN)
                decodedSymbols = HuffmanDecoder.Decode(codecDriver);
            else if(codecType == CODECTYPE_ARITHMETIC)
                decodedSymbols = ArithmeticDecoder.Decode(codecDriver);
            else
                throw new NotImplementedException();

            //decodedSymbols = codecType switch {
            //    CODECTYPE_BITLENGTH => BitlengthDecoder.Decode(codecDriver),
            //    CODECTYPE_HUFFMAN => HuffmanDecoder.Decode(codecDriver),
            //    CODECTYPE_ARITHMETIC => ArithmeticDecoder.Decode(codecDriver),
            //    _ => throw new NotImplementedException(),
            //};

            return decodedSymbols;
        }
        public static List<int> ReadVecI32(Stream data, PredictorType predictorType) {
            List<int> decodedSymbols = new List<int>(DecodeBytes(data));
            List<int> unpackedList = UnpackResiduals(decodedSymbols, predictorType);
            return unpackedList;
        }
        public static List<int> ReadVecU32(Stream data, PredictorType predictorType) {
            List<int> decodedSymbols = new List<int>(DecodeBytes(data));
            List<int> unpackedList = UnpackResiduals(decodedSymbols, predictorType);
            for (int i = 0; i < unpackedList.Count; i++) {
                unpackedList[i] = unpackedList[i] & 0xffff;
            }
            return unpackedList;
        }
        private static int PredictValue(List<int> values, int index, PredictorType predictorType) {
            int v1 = values[index - 1];
            int v2 = values[index - 2];
            int v4 = values[index - 4];

            switch (predictorType) {
                default:
                case PredictorType.PredLag1:
                case PredictorType.PredXor1:
                    return v1;

                case PredictorType.PredLag2:
                case PredictorType.PredXor2:
                    return v2;

                case PredictorType.PredStride1:
                    return (v1 + (v1 - v2));

                case PredictorType.PredStride2:
                    return (v2 + (v2 - v4));

                case PredictorType.PredStripIndex:
                    if (((v2 - v4) < 8) && ((v2 - v4) > -8)) {
                        return (v2 + (v2 - v4));
                    } else {
                        return (v2 + 2);
                    }

                case PredictorType.PredRamp:
                    return index;
            }
        }
        /**
	     * Unpacks the list of decoded symbols.
	     * @param  residuals     List of decoded symbols
	     * @param  predictorType Predictor type
	     * @return               List of unpackages integer values
	     */
        public static List<int> UnpackResiduals(List<int> residuals, PredictorType predictorType) {
            int iPredicted;
            int len = residuals.Count;

            List<int> indexList = new List<int>();

            for (int i = 0; i < len; i++) {
                if (predictorType == PredictorType.PredNull) {
                    indexList.Add(residuals[i]);

                } else {
                    // The first four values are not handeled
                    if (i < 4) {
                        indexList.Add(residuals[i]);
                    } else {
                        // Get a predicted value
                        iPredicted = PredictValue(indexList, i, predictorType);

                        // Decode the residual as the current value XOR predicted
                        if ((predictorType == PredictorType.PredXor1) || (predictorType == PredictorType.PredXor2)) {
                            indexList.Add(residuals[i] ^ iPredicted);

                            // Decode the residual as the current value plus predicted
                        } else {
                            indexList.Add(residuals[i] + iPredicted);
                        }
                    }
                }
            }

            return indexList;
        }
    }
    public class Int32ProbabilityContexts {
        public byte probabilityContextTableCount;
        public List<Int32ProbabilityContextTableEntry>[] int32ProbabilityContextTableEntries;
        public Int32ProbabilityContexts(Stream data) {
            probabilityContextTableCount = data.ReadU8();
            int32ProbabilityContextTableEntries = new List<Int32ProbabilityContextTableEntry>[probabilityContextTableCount];
            Dictionary<int, int> symbol2AssociatedValue = new Dictionary<int, int>();
            int symbolBits = 0;
            int occurrenceCountBits = 0;
            int valueBits = 0;
            int nextContextBits = 0;
            int minimumValue = 0;

            var bitSteam = new BitStream(data);
            for (int i = 0; i < probabilityContextTableCount; i++) {
                int32ProbabilityContextTableEntries[i] = new List<Int32ProbabilityContextTableEntry>();
                var probabilityContextTableEntryCount = bitSteam.ReadU32(32);
                symbolBits = bitSteam.ReadI32(6);
                occurrenceCountBits = bitSteam.ReadI32(6);
                valueBits = 0;
                if (i == 0) valueBits = bitSteam.ReadI32(6);
                nextContextBits = bitSteam.ReadI32(6);
                if (i == 0) minimumValue = bitSteam.ReadI32(32);

                for (int j = 0; j < probabilityContextTableEntryCount; ++j) {
                    var entry = new Int32ProbabilityContextTableEntry(bitSteam, symbolBits,
                        occurrenceCountBits, valueBits, nextContextBits, minimumValue);
                    if (i == 0)
                        symbol2AssociatedValue.Add(entry.symbol, entry.associatedValue);
                    else
                        entry.associatedValue = symbol2AssociatedValue[entry.symbol];
                    int32ProbabilityContextTableEntries[i].Add(entry);
                }
            }
            bitSteam.ApplyPositionToByteStream();
        }
    }
    public class Int32ProbabilityContextTableEntry {
        public int symbol;
        public int occurrenceCount;
        public int associatedValue;
        public int nextContext;
        public Int32ProbabilityContextTableEntry(BitStream bitStream,
            int symbolBits, int occurrenceCountBits,
            int valueBits, int nextContextBits, int minValue) {
            symbol = bitStream.ReadI32(symbolBits) - 2;
            occurrenceCount = bitStream.ReadI32(occurrenceCountBits);
            associatedValue = bitStream.ReadI32(valueBits) + minValue;
            nextContext = bitStream.ReadI32(nextContextBits);
        }
    }
    public class CodecDriver {
        public byte[] codeTextBytes;
        public int codeTextLengthInBits;
        public int valueElementCount;
        public int symbolCount;
        public Int32ProbabilityContexts int32ProbabilityContexts;
        public BitStream bitSteam;
        public int bitsRead;
        public int[] outOfBandValues;

        public int[] GetNextCodeText() {
            int nBits = Math.Min(32, codeTextLengthInBits - bitsRead);
            int uCodeText = bitSteam.ReadI32(nBits);
            if (uCodeText < 32)
                uCodeText <<= (32 - nBits);
            bitsRead += nBits;
            return new int[] { uCodeText, nBits };
        }

    }


}
