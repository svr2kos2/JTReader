using DLAT.JTReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JTReaderTest {
    internal class Program {
        static void Main(string[] args) {
            var jtFile = "E:\\workspace\\JTFiles\\robot_jt8.2.jt";
            //var jtFile = "E:\\workspace\\JTFiles\\example_block_jt9.5.jt";
            //var jtFile = "E:\\workspace\\JTFiles\\example_block_jt10.3.jt";
            var file = new JTFile(jtFile);
            
           
            //var jtFileFolder = "E:\\workspace\\JTFiles";
            //foreach(var jtFile in Directory.GetFiles(jtFileFolder)) {
            //    var file = new JTFile(jtFile);
            //    //Console.WriteLine(file.version);
            //    //var lsg = new LSG(file.LSGSegment);
            //}
            //Console.ReadKey();
        }
    }
}
