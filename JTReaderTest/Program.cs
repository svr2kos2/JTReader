using DLAT.JTReader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace JTReaderTest {
    
    internal class Program {
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
        }
 
        [DllImport("kernel32.dll")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);
        static void Main(string[] args) {
            //var jtFile = "E:\\workspace\\JTFiles\\robot_jt8.2.jt";
            //var jtFile = "E:\\workspace\\JTFiles\\example_block_jt9.5.jt";
            //var jtFile = "E:\\workspace\\JTFiles\\opening_protection_plate1_jt9.5.jt";
            //var jtFile = "E:\\workspace\\JTFiles\\example_block_jt10.3.jt";
            //var jtFile = "E:\\workspace\\JTFiles\\gre_he_30d004003_jt10.0.jt";
            
            //var jf =
            //    "C:\\Users\\root\\Desktop\\share\\LIB_ROOT\\Hefei\\VW316_8CM\\02_UB2\\U2A1\\KONZEPT_BEMI\\WERKZEUGE\\ZUBEHOER\\UB2 Camera for respot  20220114.cojt\\UB2 Camera for respot  20220114.jt";

            //var g = new JTFile(jf);
            //return;

            JTFile.InitNativeLibrary();

            ConcurrentDictionary<string, JTFile> jtFiles = new ConcurrentDictionary<string, JTFile>();
            
            var startTime = DateTime.Now;
            
            
            //var jtFileDirectory = "C:\\Users\\root\\Desktop\\share\\LIB_ROOT";
            //var jtFileDirectory = "E:\\workspace\\JTFiles\\robot";
            var jtFileDirectory = "E:\\workspace\\JTFiles";
            //var jtFileDirectory = "E:\\workspace\\JTDump";
            
            Queue<string> dirs = new Queue<string>();
            dirs.Enqueue(jtFileDirectory);

            //var readers = new List<Task<LSG>>();
            
            
            for (; dirs.Count != 0;) {
                var dir = dirs.Dequeue();
                var files = Directory.GetFiles(dir);
                foreach (var jtFile in files) {
                    if (jtFile.EndsWith(".jt")) {
                        var task = Task.Run(() => {
                            
                        });
                        var f = new JTFile(jtFile);
                        var l =  new LSG(f);
                        //return l;
                        //task.Wait();
                        //readers.Add(task);
                    }
                }

                //if (fileCnt > 20)
                //    break;
                var subDirs = Directory.GetDirectories(dir);
                foreach (var subDir in subDirs) 
                    dirs.Enqueue(subDir);
            }
            ClearMemory();
            //DLAT.JTReader.Debug.Log("#gAll task created. waiting...#w");
            
            //Task.WaitAll(readers.ToArray());

            //for (;;) {
            //    Task.Delay(1000).Wait();
            //    bool allCompleted = true;
            //    foreach (var reader in readers) {
            //        var (task, file) = reader;
            //        if (!task.IsCompleted) {
            //            allCompleted = false;
            //            Console.WriteLine("Wait " + file);   
            //        }

            //    }

            //    if (allCompleted)
            //        break;
            //}
            
            // var lsgs = new List<LSG>();
            // foreach (var r in readers) {
            //     lsgs.Add(r.Result);
            // }
            
            
            //DLAT.JTReader.Debug.Log("#gAll completed.#w" + (DateTime.Now - startTime).TotalSeconds);
            //
            // foreach (var reader in readers) {
            //     var task = reader;
            //     if(task.IsFaulted)
            //         Console.WriteLine(task.Exception);
            // }
            
            
            
            //var file = new JTFile(jtFile);


            //var jtFileFolder = "E:\\workspace\\JTFiles";
            //foreach(var jtFile in Directory.GetFiles(jtFileFolder)) {
            //    var file = new JTFile(jtFile);
            //    //Console.WriteLine(file.version);
            //    //var lsg = new LSG(file.LSGSegment);
            //}
            //Console.ReadKey();
            //Console.ReadKey();
        }
    }
}
