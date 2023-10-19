using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using System.Collections;
using System.Threading;
using System.Collections.ObjectModel;


//This is the main data model
//This is where we are actually executing the logic of the assignment
namespace MDTGenerators
{
    public class JSONGenerator {
        public string name { get; set; }
        public int interval { get; set; }
        public string operation { get; set; }

        public JSONGenerator(string Name, int Interval, string Operation) {
            name = Name;
            interval = Interval;
            operation = Operation;
        }

        public JSONGenerator() {
            name = "";
            interval = 1;
            operation = "";
        }
    }

    public class JSONData {
        public object[][] datasets { get; set; }
        public JSONGenerator[] generators {get;set;}
    }

    public class GeneratorMain {
        private List<string> outputLines = new List<string>();
        private List<string> newOutputLines = new List<string>();

        private readonly object linesLock = new object();
        private readonly object newLinesLock = new object();
        private ObservableCollection<Generator> generators = new ObservableCollection<Generator>();
        private List<Thread> generatorThreads = new List<Thread>();
        private object[][] dataSets = { };

        public ObservableCollection<Generator> Generators {
            get {
                return generators;
            }
            //no set on purpose
        }

        public void ExecuteGenerator(object generator) {
            if (generator is Generator) {
                while (!((Generator)generator).IsDone()) {
                    string newLine = ((Generator)generator).getCalcString();
                    AddOutputLines(newLine);
                    WaitIfNotDone((Generator)generator);
                }
            }
        }

        private void AddOutputLines(string newLine) {
            lock (linesLock) {
                outputLines.Add(newLine);
            }
            lock (newLinesLock) {
                newOutputLines.Add(newLine);
            }
        }

        private void WaitIfNotDone(Generator generator) {
            if (!(generator).IsDone()) {
                generator.Wait();
            }
        }

        public void UpdateDataSets(object[][] DataSets) {
            dataSets = DataSets;
            foreach (Generator gen in generators) {
                gen.SetDataSets(DataSets);
            }
        }

        public void Clear() {
            dataSets = new object[0][];
            generators.Clear();
            generatorThreads.Clear();
        }

        public void LoadJSON(string JSONString) {
            var jsonData = System.Text.Json.JsonSerializer.Deserialize<JSONData>(JSONString);
            dataSets = jsonData.datasets;  //saving this for ease of step 2
            int maxColumns = GetMaxColumnsFromDataSet();
            EnsureDataSetRowsAreSameLength(maxColumns); //the solution ends up a little cleaner if we set all arrays to the same length, padded with nulls
            ClearGenerators();
            CreateGeneratorsFromJSONData(jsonData);
        }

        private void ClearGenerators() {
            generators.Clear();
            generatorThreads.Clear();
        }

        private int GetMaxColumnsFromDataSet() {
            int maxColumns = 0;
            foreach (object[] row in dataSets) {
                maxColumns = row.Length > maxColumns ? row.Length : maxColumns;
            }
            return maxColumns;
        }

        private void EnsureDataSetRowsAreSameLength(int maxColumns) {
            for (int rowIndex = 0; rowIndex < dataSets.Length; rowIndex++) {
                EnsureOneDataSetRowIsMaxLength(rowIndex, maxColumns);
            }
        }

        private void EnsureOneDataSetRowIsMaxLength(int rowIndex, int maxColumns) {
            object[] newRow = new object[maxColumns];
            newRow = CopyDataSetValuesToNewRowWithPadding(newRow, rowIndex, maxColumns);
            dataSets[rowIndex] = newRow;
        }

        private object[] CopyDataSetValuesToNewRowWithPadding(object[] newRow, int rowIndex, int maxColumns) {
            int columnIndex = 0;
            for (; columnIndex < dataSets[rowIndex].Length && columnIndex < maxColumns; columnIndex++) {
                newRow[columnIndex] = GetObjectValueFromDataSet(rowIndex, columnIndex);
            }
            for (; columnIndex < maxColumns; columnIndex++) {
                newRow[columnIndex] = null;
            }
            return newRow;
        }
     
        private object GetObjectValueFromDataSet(int rowIndex, int columnIndex) {
            if (dataSets[rowIndex][columnIndex] is JsonElement) {                
                return ((JsonElement)dataSets[rowIndex][columnIndex]).GetSingle();// if the object is a JsonElement then convert it to float
            }
            else {
                return dataSets[rowIndex][columnIndex];
            }
        }

        private void CreateGeneratorsFromJSONData(JSONData jsonData) {
            foreach (JSONGenerator gen in jsonData.generators) {
                switch (gen.operation.ToUpper()) {
                    case "SUM":
                        generators.Add(new GeneratorSum(gen.name, gen.interval, dataSets));
                        generatorThreads.Add(new Thread(new ParameterizedThreadStart(ExecuteGenerator)));
                        break;
                    case "AVERAGE":
                    case "AVG":
                        generators.Add(new GeneratorAverage(gen.name, gen.interval, dataSets));
                        generatorThreads.Add(new Thread(new ParameterizedThreadStart(ExecuteGenerator)));
                        break;
                    case "MIN":
                        generators.Add(new GeneratorMin(gen.name, gen.interval, dataSets));
                        generatorThreads.Add(new Thread(new ParameterizedThreadStart(ExecuteGenerator)));
                        break;
                    case "MAX":
                        generators.Add(new GeneratorMax(gen.name, gen.interval, dataSets));
                        generatorThreads.Add(new Thread(new ParameterizedThreadStart(ExecuteGenerator)));
                        break;
                }
            }
        }

        public void AddColumnToDataSet() {
            AdjustColumnsInDataSet(+1);
        }

        public void RemoveColumnFromDataSet() {
            AdjustColumnsInDataSet(-1);
        }

        private void AdjustColumnsInDataSet(int adjustmentToNumberOfColumns) {
            for (int rowIndex = 0; rowIndex < dataSets.Length; rowIndex++) {
                int newRowLength = dataSets[rowIndex].Length + adjustmentToNumberOfColumns;
                if (newRowLength < 0) newRowLength = 0;
                object[] newRow = new object[newRowLength];
                newRow = CopyDataSetValuesToNewRowWithPadding(newRow, rowIndex, newRowLength);
                dataSets[rowIndex] = newRow;
            }
        }

        public void CheckAndCorrectAllGeneratorsTypes() {
            for(int index = 0; index < Generators.Count(); index++) {
                if (!((Generators[index].GetType().ToString().ToUpper().Contains(Generators[index].Operation.ToUpper())) ||
                      ((Generators[index] is GeneratorAverage) && Generators[index].Operation.ToUpper() == "AVG")))  { //accept AVG for average
                    ConvertGenerator(index, Generators[index].Operation);
                }
            }
        }

        private void ConvertGenerator(int index, string operation) {
           if ((index >= 0) && (index < generators.Count)) {
                switch (operation.ToUpper()) {
                    case "SUM":
                        generators[index] = new GeneratorSum(generators[index].Name, generators[index].Interval, dataSets);
                        break;
                    case "AVERAGE":
                    case "AVG":
                        generators[index] = new GeneratorAverage(generators[index].Name, generators[index].Interval, dataSets);
                        break;
                    case "MIN":
                        generators[index] = new GeneratorMin(generators[index].Name, generators[index].Interval, dataSets);
                        break;
                    case "MAX":
                        generators[index] = new GeneratorMax(generators[index].Name, generators[index].Interval, dataSets);
                        break;
                }
            }
        }

        public void SetInterval(int index, int interval) {
            if ((index >= 0) && (index < generators.Count)) {
                generators[index].Interval = interval;
            }
        }

        public void ExecuteGenerators() {
            //we should have 1 for 1 generators to generator threads...if not, someone messed up the coding above!
            for (int index = 0; index < generators.Count; index++) {
                CheckAndFixThreadState(index);
                generatorThreads[index].Start(generators[index]);
            }
        }

        private void CheckAndFixThreadState(int threadIndex) {
            if (generatorThreads[threadIndex].ThreadState != ThreadState.Unstarted) {
                generatorThreads[threadIndex] = new Thread(new ParameterizedThreadStart(ExecuteGenerator));
            }
        }
        
        public bool IsAllGeneratorsDone() {
            foreach(Thread thread in generatorThreads) {
                if (thread.IsAlive)
                    return false;
            }
            return true;
        }

        public ArrayList GetLinesArrayList() {
            ArrayList returnLines = new ArrayList();
            lock (linesLock) {
                returnLines = new ArrayList(outputLines);
            }
            return returnLines;
        }

        public ArrayList GetNewLinesArrayList() {
            ArrayList returnLines = new ArrayList();
            lock (newLinesLock) {
                returnLines = new ArrayList(newOutputLines);
                newOutputLines.Clear();
            }
            return returnLines;
        }

        public List<string> GetLines() {
            List<string> returnLines = new List<string>();
            lock (linesLock) {
                returnLines = new List<string>(outputLines);
            }
            return returnLines;
        }

        public List<string> GetNewLines() {
            List<string> returnLines = new List<string>();
            lock (newLinesLock) {
                returnLines = new List<string>(newOutputLines);
                newOutputLines.Clear();
            }
            return returnLines;
        }

        public object[][] GetDataSets() {
            return dataSets;
        }

        public JSONGenerator[] GetJSONGeneratorValues() {
            JSONGenerator[] returnJSONGenerators = new JSONGenerator[generators.Count];
            for (int index = 0; index < generators.Count; index++) {
                returnJSONGenerators[index] = new JSONGenerator(generators[index].Name, generators[index].Interval, generators[index].Operation);
            }
            return returnJSONGenerators;
        }

        public void Reset() {
            foreach(Generator gen in generators) {
                gen.Reset();
            }
        }
    }
}
