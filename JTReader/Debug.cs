﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace DLAT.JTReader {
    public static class Debug {
        public static bool cache = false;
        private static List<(string, int)> cachedLogs = new List<(string, int)>();
        public static void Log(string str,int ali = 0) {
            if (cache) {
                cachedLogs.Add((str, ali));
                return;   
            }
            
            for (int i = 0; i < ali; ++i)
                Console.Write("    ");
            var s = str.Split('#');
            if(s.Length < 2) {
                Console.WriteLine(s[0]);
                return;
            }
            Console.Write(s[0]);
            for (int i = 1; i < s.Length; ++i) {
                var c = s[i][0];
                switch (c) {
                    case 'r': Console.ForegroundColor = ConsoleColor.Red; break;
                    case 'g': Console.ForegroundColor = ConsoleColor.Green; break;
                    case 'b': Console.ForegroundColor = ConsoleColor.Blue; break;
                    case 'w': Console.ForegroundColor = ConsoleColor.White; break;
                }
                Console.Write(s[i].Substring(1));
                Console.ForegroundColor = ConsoleColor.White;
            }
            Console.WriteLine();
        }

        public static void FlushLogs() {
            cache = false;
            foreach (var (str,ali) in cachedLogs) {
                Log(str, ali);
            }
            cachedLogs.Clear();
        }
    }
}
