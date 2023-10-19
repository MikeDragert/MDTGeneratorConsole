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
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("interval")]
        public int Interval { get; set; }
        [JsonPropertyName("operation")]
        public string Operation { get; set; }

        public JSONGenerator(string name, int interval, string operation) {
            Name = name;
            Interval = interval;
            Operation = operation;
        }

        public JSONGenerator() {
            Name = "";
            Interval = 1;
            Operation = "";
        }
    }

    public class JSONData {
        [JsonPropertyName("datasets")]
        public object[][] Datasets { get; set; }
        [JsonPropertyName("generators")]
        public JSONGenerator[] Generators {get;set;}
    }

    public class GeneratorMain {
        private List<string> _outputLines = new List<string>();
        private List<string> _newOutputLines = new List<string>();

        private readonly object _linesLock = new object();
        private readonly object _newLinesLock = new object();
        private ObservableCollection<Generator> _generators = new ObservableCollection<Generator>();
        private List<Thread> _generatorThreads = new List<Thread>();
        private object[][] _dataSets = { };

        public ObservableCollection<Generator> Generators {
            get {
                return _generators;
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
            lock (_linesLock) {
                _outputLines.Add(newLine);
            }
            lock (_newLinesLock) {
                _newOutputLines.Add(newLine);
            }
        }

        private void WaitIfNotDone(Generator generator) {
            if (!(generator).IsDone()) {
                generator.Wait();
            }
        }

        public void UpdateDataSets(object[][] dataSets) {
            _dataSets = dataSets;
            foreach (Generator gen in _generators) {
                gen.SetDataSets(_dataSets);
            }
        }

        public void Clear() {
            _dataSets = new object[0][];
            _generators.Clear();
            _generatorThreads.Clear();
        }

        public void LoadJSON(string jsonString) {
            var jsonData = System.Text.Json.JsonSerializer.Deserialize<JSONData>(jsonString);
            _dataSets = jsonData.Datasets;  //saving this for ease of step 2
            int maxColumns = GetMaxColumnsFromDataSet();
            EnsureDataSetRowsAreSameLength(maxColumns); //the solution ends up a little cleaner if we set all arrays to the same length, padded with nulls
            ClearGenerators();
            CreateGeneratorsFromJSONData(jsonData);
        }

        private void ClearGenerators() {
            _generators.Clear();
            _generatorThreads.Clear();
        }

        private int GetMaxColumnsFromDataSet() {
            int maxColumns = 0;
            foreach (object[] row in _dataSets) {
                maxColumns = row.Length > maxColumns ? row.Length : maxColumns;
            }
            return maxColumns;
        }

        private void EnsureDataSetRowsAreSameLength(int maxColumns) {
            for (int rowIndex = 0; rowIndex < _dataSets.Length; rowIndex++) {
                EnsureOneDataSetRowIsMaxLength(rowIndex, maxColumns);
            }
        }

        private void EnsureOneDataSetRowIsMaxLength(int rowIndex, int maxColumns) {
            object[] newRow = new object[maxColumns];
            newRow = CopyDataSetValuesToNewRowWithPadding(newRow, rowIndex, maxColumns);
            _dataSets[rowIndex] = newRow;
        }

        private object[] CopyDataSetValuesToNewRowWithPadding(object[] newRow, int rowIndex, int maxColumns) {
            int columnIndex = 0;
            for (; columnIndex < _dataSets[rowIndex].Length && columnIndex < maxColumns; columnIndex++) {
                newRow[columnIndex] = GetObjectValueFromDataSet(rowIndex, columnIndex);
            }
            for (; columnIndex < maxColumns; columnIndex++) {
                newRow[columnIndex] = null;
            }
            return newRow;
        }
     
        private object GetObjectValueFromDataSet(int rowIndex, int columnIndex) {
            if (_dataSets[rowIndex][columnIndex] is JsonElement) {                
                return ((JsonElement)_dataSets[rowIndex][columnIndex]).GetSingle();// if the object is a JsonElement then convert it to float
            }
            else {
                return _dataSets[rowIndex][columnIndex];
            }
        }

        private void CreateGeneratorsFromJSONData(JSONData jsonData) {
            foreach (JSONGenerator gen in jsonData.Generators) {
                switch (gen.Operation.ToUpper()) {
                    case "SUM":
                        _generators.Add(new GeneratorSum(gen.Name, gen.Interval, _dataSets));
                        _generatorThreads.Add(new Thread(new ParameterizedThreadStart(ExecuteGenerator)));
                        break;
                    case "AVERAGE":
                    case "AVG":
                        _generators.Add(new GeneratorAverage(gen.Name, gen.Interval, _dataSets));
                        _generatorThreads.Add(new Thread(new ParameterizedThreadStart(ExecuteGenerator)));
                        break;
                    case "MIN":
                        _generators.Add(new GeneratorMin(gen.Name, gen.Interval, _dataSets));
                        _generatorThreads.Add(new Thread(new ParameterizedThreadStart(ExecuteGenerator)));
                        break;
                    case "MAX":
                        _generators.Add(new GeneratorMax(gen.Name, gen.Interval, _dataSets));
                        _generatorThreads.Add(new Thread(new ParameterizedThreadStart(ExecuteGenerator)));
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
            for (int rowIndex = 0; rowIndex < _dataSets.Length; rowIndex++) {
                int newRowLength = _dataSets[rowIndex].Length + adjustmentToNumberOfColumns;
                if (newRowLength < 0) newRowLength = 0;
                object[] newRow = new object[newRowLength];
                newRow = CopyDataSetValuesToNewRowWithPadding(newRow, rowIndex, newRowLength);
                _dataSets[rowIndex] = newRow;
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
           if ((index >= 0) && (index < _generators.Count)) {
                switch (operation.ToUpper()) {
                    case "SUM":
                        _generators[index] = new GeneratorSum(_generators[index].Name, _generators[index].Interval, _dataSets);
                        break;
                    case "AVERAGE":
                    case "AVG":
                        _generators[index] = new GeneratorAverage(_generators[index].Name, _generators[index].Interval, _dataSets);
                        break;
                    case "MIN":
                        _generators[index] = new GeneratorMin(_generators[index].Name, _generators[index].Interval, _dataSets);
                        break;
                    case "MAX":
                        _generators[index] = new GeneratorMax(_generators[index].Name, _generators[index].Interval, _dataSets);
                        break;
                }
            }
        }

        public void SetInterval(int index, int interval) {
            if ((index >= 0) && (index < _generators.Count)) {
                _generators[index].Interval = interval;
            }
        }

        public void ExecuteGenerators() {
            //we should have 1 for 1 generators to generator threads...if not, someone messed up the coding above!
            for (int index = 0; index < _generators.Count; index++) {
                CheckAndFixThreadState(index);
                _generatorThreads[index].Start(_generators[index]);
            }
        }

        private void CheckAndFixThreadState(int threadIndex) {
            if (_generatorThreads[threadIndex].ThreadState != ThreadState.Unstarted) {
                _generatorThreads[threadIndex] = new Thread(new ParameterizedThreadStart(ExecuteGenerator));
            }
        }
        
        public bool IsAllGeneratorsDone() {
            foreach(Thread thread in _generatorThreads) {
                if (thread.IsAlive)
                    return false;
            }
            return true;
        }

        public ArrayList GetLinesArrayList() {
            ArrayList returnLines = new ArrayList();
            lock (_linesLock) {
                returnLines = new ArrayList(_outputLines);
            }
            return returnLines;
        }

        public ArrayList GetNewLinesArrayList() {
            ArrayList returnLines = new ArrayList();
            lock (_newLinesLock) {
                returnLines = new ArrayList(_newOutputLines);
                _newOutputLines.Clear();
            }
            return returnLines;
        }

        public List<string> GetLines() {
            List<string> returnLines = new List<string>();
            lock (_linesLock) {
                returnLines = new List<string>(_outputLines);
            }
            return returnLines;
        }

        public List<string> GetNewLines() {
            List<string> returnLines = new List<string>();
            lock (_newLinesLock) {
                returnLines = new List<string>(_newOutputLines);
                _newOutputLines.Clear();
            }
            return returnLines;
        }

        public object[][] GetDataSets() {
            return _dataSets;
        }

        public JSONGenerator[] GetJSONGeneratorValues() {
            JSONGenerator[] returnJSONGenerators = new JSONGenerator[_generators.Count];
            for (int index = 0; index < _generators.Count; index++) {
                returnJSONGenerators[index] = new JSONGenerator(_generators[index].Name, _generators[index].Interval, _generators[index].Operation);
            }
            return returnJSONGenerators;
        }

        public void Reset() {
            foreach(Generator gen in _generators) {
                gen.Reset();
            }
        }
    }
}
