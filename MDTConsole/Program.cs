using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using MDTGenerators;
using System.Threading;
using System.IO;

namespace MDTConsole
{
    class Program
    {
        private const string _JSONFileName = @"E:\Mike's Document's and Files\MDT\data.json"; //ideally this would come from a config file

        static void Main(string[] args) {
            string json = File.ReadAllText(_JSONFileName);
            GeneratorMain main = new GeneratorMain();
            main.LoadJSON(json);
            main.ExecuteGenerators();
            WhileNotDoneCheckAndPrintNewLines(main);
            
            //when done, let's wait until user presses enter! //not exactly part of the instructions, but figured it would be helpful
            Console.Out.WriteLine("Press enter to continue");
            Console.ReadLine();
        }

        static void WhileNotDoneCheckAndPrintNewLines(GeneratorMain main) {
            ArrayList nextLines = new ArrayList();
            while (!main.IsAllGeneratorsDone()) {
                CheckAndPrintNewLines(main);
                Thread.Sleep(50);
            }           
            CheckAndPrintNewLines(main);//one last time, in case lines were added between call to getnewlines and the check to isDone
        }

        static void CheckAndPrintNewLines(GeneratorMain main) {
            List<string> nextLines = main.GetNewLines();
            foreach(string line in nextLines) {
                Console.Out.WriteLine(line);
            }            
        }
    }
}
