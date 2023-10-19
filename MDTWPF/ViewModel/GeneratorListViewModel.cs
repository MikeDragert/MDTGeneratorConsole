using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDTGenerators;
using System.IO;
using System.Collections.ObjectModel;
using System.Collections;
using System.Windows.Input;
using System.ComponentModel;
using System.Threading;
using System.Data;
using System.Windows.Controls;


namespace MDTWPF.ViewModel
{
    //this is the main view model for the generator data
    public class GeneratorListViewModel : INotifyPropertyChanged
    {
        GeneratorMain generatorMain = new GeneratorMain();  //generator main has the business logic and generator data - this is the data model       
        public ResultsViewModel resultsViewModel = new ResultsViewModel();
        private DataTable dataSetTable;
        private Thread generatorThread;
        private string fileName = "";
        private string prevFileName = "";
        private DataTable prevDataSetTable;

        public ResultsViewModel Results {
            get {
                return resultsViewModel;
            }
            set {
                resultsViewModel = value;
            }
        }

        public ObservableCollection<Generator> Generators {
            get {
                return generatorMain.Generators;
            }
            set {
                //leave set blank, update through methods
            }
        }

        public string FileName {
            get {
                return fileName;
            }
            set {
                fileName = value;
                OnPropertyChanged("FileName");
            }
        }

        private void ExecutingAllGenerators() {
            ClearResults();
            generatorMain.ExecuteGenerators();
            WhileNotDoneCheckForNewLinesAndAddToResults();            
        }

        private void ClearResults() {
            resultsViewModel.Results = "";
            OnPropertyChanged("Results");
        }

        private void WhileNotDoneCheckForNewLinesAndAddToResults() {
            while (!generatorMain.IsAllGeneratorsDone()) {
                CheckForNewLinesAndAddToResults();
                Thread.Sleep(50);
            }
            CheckForNewLinesAndAddToResults();  //one last time just in case
        }

        private void CheckForNewLinesAndAddToResults() {
            List<string> nextLines = generatorMain.GetNewLines();
            if (nextLines.Count > 0) {
                for (int index = 0; index < nextLines.Count; index++) {
                    if (nextLines[index] is string) {
                        resultsViewModel.Results += nextLines[index] + System.Environment.NewLine;
                    }
                }
                OnPropertyChanged("Results");
            }
        }

        private bool LoadJSONToGenerators(string fileName) {
            try {
                ReadJSONFileAndLoadToGenerator(fileName);
                return true;
            }
            catch (Exception ex) {
                //for now let's not worry about what this is.
                // this should really be changed to throw an exception rather than return a bool result
                return false;
            }
        }

        private void ReadJSONFileAndLoadToGenerator(string fileName) {
            string json = File.ReadAllText(fileName);
            generatorMain.LoadJSON(json);
        }

        public GeneratorListViewModel() {
            fileName = GetNoFileMessage();
        }

        private void RefreshDataSetTableFromGeneratorObjectArray() {

            object[][] retrievedDataSets = generatorMain.GetDataSets();
            GetDataTableFromRetrievedDataSet(retrievedDataSets);
            OnPropertyChanged("DataSetTable");
        }

        private void GetDataTableFromRetrievedDataSet(object[][] retrievedDataSets) {
            int maxRowColumnCount = GetMaxRowColumnCount(retrievedDataSets);
            dataSetTable = new DataTable();
            EnsureDataSetTableHasMaxColumns(maxRowColumnCount);
            CreateAndAddDataRows(retrievedDataSets, maxRowColumnCount);
        }

        private int GetMaxRowColumnCount(object[][] retrievedDataSets) {
            int maxRowColumnCount = 0;
            foreach (object[] dataSet in retrievedDataSets)
            {
                if (dataSet.Length > maxRowColumnCount) maxRowColumnCount = dataSet.Length;
            }
            return maxRowColumnCount;
        }
        
        private void EnsureDataSetTableHasMaxColumns(int columnCount) {
            int columnIndex = 0;
            while (dataSetTable.Columns.Count < columnCount) {
                columnIndex++;
                DataColumn newColumn = CreateNewColumn(columnIndex);
                dataSetTable.Columns.Add(newColumn);
            }
        }

        private DataColumn CreateNewColumn(int columnIndex) {
            DataColumn newColumn = new DataColumn();
            newColumn.ColumnName = "Column" + columnIndex;
            newColumn.DataType = System.Type.GetType("System.Single");  //once we switched to object array instead of float array, had to explicitly set the datatype in the DataTable, or else would default to string
            return newColumn;
        }

        private void CreateAndAddDataRows(object[][] retrievedDataSets, int maxRowColumnCountRow) {
            foreach (object[] rowDataSet in retrievedDataSets) {
                object[] newDataRowArrayArray = CreateAndCopyDataRow(rowDataSet, maxRowColumnCountRow);               
                dataSetTable.Rows.Add(newDataRowArrayArray);
            }
        }

        private object[] CreateAndCopyDataRow(object[] rowDataSet, int maxRowColumnCountRow) {
            object[] newDataRowArrayArray = new object[maxRowColumnCountRow];  //decided to make all rows max length on purpose so it isn't jagged, we will accomodate null objects
            for (int index = 0; index < rowDataSet.Length; index++) {
                newDataRowArrayArray[index] = rowDataSet[index];
            }
            return newDataRowArrayArray;
        }

        public DataTable DataSetTable {
            get {
                if (dataSetTable is null) RefreshDataSetTableFromGeneratorObjectArray();  //maked sure datatable is populated based on the actual data in our model!!
                return dataSetTable;
            }
            set {
                dataSetTable = value;
                SendDataSetTableToGeneratorMain();
                OnPropertyChanged("DataSetTable");
            }
        }
        
        private void SendDataSetTableToGeneratorMain() {
            if (!(dataSetTable is null))
            {
                object[][] newDataSetObjectArray = CreateNewObjectArrayFromDataSetTable();
                generatorMain.Reset();  //if we ran the generators, need to reset before changing data
                generatorMain.UpdateDataSets(newDataSetObjectArray);
            }
        }

        private object[][] CreateNewObjectArrayFromDataSetTable() {
            object[][] newDataSetObjectArray = new object[dataSetTable.Rows.Count][];
            for (int rIndex = 0; rIndex < dataSetTable.Rows.Count; rIndex++) {
                newDataSetObjectArray[rIndex] = dataSetTable.Rows[rIndex].ItemArray;
            }
            return newDataSetObjectArray;
        }
       
        private ICommand generateClickCommand;
        public ICommand GenerateClickCommand {
            get {
                return generateClickCommand ?? (generateClickCommand = new CommandHandler(() => ExecuteAllGenerators(), true));
            }
        }

        private ICommand addClickCommand;
        public ICommand AddClickCommand {
            get {
                return addClickCommand ?? (addClickCommand = new CommandHandler(() => AddColumnToDataTable(), true));
            }
        }

        private ICommand removeClickCommand;
        public ICommand RemoveClickCommand {
            get {
                return removeClickCommand ?? (removeClickCommand = new CommandHandler(() => RemoveColumnFromDataTable(), true));
            }
        }

        private ICommand selectFileCommand;
        public ICommand SelectFileCommand {
            get {
                return selectFileCommand ?? (selectFileCommand = new CommandHandler(() => SelectJSONFileForGenerators(), true));
            }
        }

        public void ExecuteAllGenerators() {
            generatorMain.Reset();
            //I had intended for the update to the table to get pushed to the generatorMain when the data is updated with "Set"
            //BUT that is not working, so do it here before we use the data
            SendDataSetTableToGeneratorMain();
            generatorMain.CheckAndCorrectAllGeneratorsTypes();  //in case it was changed
            StartNewGeneratorExecutionThread();
        }

        public void StartNewGeneratorExecutionThread() { 
            generatorThread = new Thread(ExecutingAllGenerators);
            generatorThread.Start();
        }

        public void AddColumnToDataTable() {
            DataColumn newColumn = CreateNewColumn(dataSetTable.Columns.Count + 1);
            dataSetTable.Columns.Add(newColumn);
            generatorMain.AddColumnToDataSet();
            OnPropertyChanged("DataSetTable");
        }

        public void RemoveColumnFromDataTable() {
            if (dataSetTable.Columns.Count > 0) {
                dataSetTable.Columns.RemoveAt(dataSetTable.Columns.Count - 1);
            }
            generatorMain.RemoveColumnFromDataSet();
            OnPropertyChanged("DataSetTable");
        }

        //get and load a file
        public void SelectJSONFileForGenerators() {
            SaveCurrentValuesToPreviousForFileSelection();

            fileName = GetNoFileMessage();
            Microsoft.Win32.OpenFileDialog dlg = CreateAndInitializeDialogBoxForFileSelection(); 
            Nullable<bool> dlgResult = dlg.ShowDialog();

            if (IsFileOpenGoodStatus(dlgResult, dlg.FileName)) {
                AttemptLoadJSONFileToGenerators(dlg.FileName);                          
            }
            else {
                ResetCurrentValuesFromPreviousForFileSelection();
            }
            RefreshDataSetTableFromGeneratorObjectArray(); //let's refresh no matter what
            OnPropertyChanged("FileName");
            OnPropertyChanged("Results");
        }

        private void SaveCurrentValuesToPreviousForFileSelection() {
            prevFileName = fileName;
            prevDataSetTable = dataSetTable; 
        }

        private void ResetCurrentValuesFromPreviousForFileSelection() {
            fileName = prevFileName;
            DataSetTable = prevDataSetTable;
        }

        private bool IsFileOpenGoodStatus(Nullable<bool> dlgResult, string fileName) {
            return ((dlgResult == true) &&
                    (fileName.ToLower().EndsWith(".json")));
        }

        private string GetNoFileMessage() {
            return "Select file...";
        }

        private void AttemptLoadJSONFileToGenerators(string JSONFileName){
            InitializeForLoadJSONFileToGenerators(JSONFileName);
            if (!LoadJSONToGenerators(fileName)) {
                ResetCurrentValuesFromPreviousForFileSelection();
            }
        }

        private void InitializeForLoadJSONFileToGenerators(string JSONFileName) {
            fileName = JSONFileName;
            generatorMain.Clear();
            DataSetTable = null;
            resultsViewModel.Results = "";
        }

        private Microsoft.Win32.OpenFileDialog CreateAndInitializeDialogBoxForFileSelection() {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".json";
            dlg.Filter = "All Files|*.*";
            return dlg;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class CommandHandler : ICommand
    {
        private Action _commandAction;
        private bool _commandCanExecute;
        public CommandHandler(Action commandAction, bool commandCanExecute) {
            _commandAction = commandAction;
            _commandCanExecute = commandCanExecute;
        }

        public bool CanExecute(object parameter) {
            return _commandCanExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter) {
            _commandAction();
        }
    }
}
