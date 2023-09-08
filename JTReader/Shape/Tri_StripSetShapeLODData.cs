using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLAT.JTReader {
    public class Tri_StripSetShapeLODData {
        public static string typeID = "10DD10AB-2AC8-11D1-9B-6B-00-80-C7-BB-59-97";
        public VertexShapeLODData vertexShapeLodData;
        public short versionNumber;
        public VertexBasedShapeCompressedRepData vertexBasedShapeData;
        
        public Tri_StripSetShapeLODData(Element ele) {
            var data = ele.dataStream;
            vertexShapeLodData = new VertexShapeLODData(ele);
            versionNumber = data.ReadVersionNumber();
            if (ele.majorVersion < 9) {
                vertexBasedShapeData = new VertexBasedShapeCompressedRepData(data);
            }



            Console.WriteLine(GetVertices().Count);
        }
        
        public List<float> GetVertices() {
            if(vertexBasedShapeData != null)
                return vertexBasedShapeData.GetVertices();
            return vertexShapeLodData.GetVertices();
        }
    }
}
