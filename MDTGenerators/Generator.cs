using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json.Serialization;

namespace MDTGenerators
{
    //base class for each generator
    // new generator types need to inherit from this, and supply the calc() function
    public abstract class Generator
    {
        protected string _Name = "";
        protected int _Interval = 0;
        protected string _Operation = ""; //initially this was not supposed to be needed, we know the type based on the class
                                         //but step 2 dictates that we have this field.
                                         // it will be initialized to match the type of class loaded
                                         //  and if the viewmodel changes it, GeneratorMain needs to correct the generator type before executing!!!
        protected object[][] _Data = { };
        protected int _CurrentDataRowSet = 0;  //this is the current data set row that this generator is working on as part of generation
                                            // it should increment with each new line of data that the generator calculates

        public enum GeneratorTypes {
            sum,
            average,
            min,
            max
        }

        
        public string Name {
            get {
                return _Name;
            }
            set {
                _Name = value;
            }
        }

        public int Interval {
            get {
                return _Interval;
            }
            set {
                _Interval = value;
            }
        }

        public string Operation {
            get {
                return _Operation;
            }
            set {                
                if (_Operation != value) {  //we actually need to validate the operation before accepting it
                    if (ValidOperation(value)) {
                        _Operation = value;
                    }
                }
            }
        }

        public Generator(string name, int interval, object[][] data) {
            _Name = name;           
            _Interval = interval > 0 ? interval : 0; //seems like a negative interval would not make sense...
            _Data = data;
            _CurrentDataRowSet = 0;
        }

        public abstract float CalculateResults();

        public bool ValidOperation(string operation) {
            if (Enum.IsDefined(typeof(GeneratorTypes), operation.ToLower())) {
                return true;
            }            
            //should have covered most types with the enum, but want to allow both AVG and average and "AVG" is not in the enum
            return operation.ToLower() == "avg";
        }

        public string getCalcString() {
            return DateTime.Now.ToString("hh:mm:ss") +" " + _Name + " " + this.CalculateResults().ToString();
        }
        
        public void Wait() {
            Thread.Sleep(_Interval * 1000);
        }

        public void Reset() {
            _CurrentDataRowSet = 0;  
        }

        public bool IsDone() {
            return _CurrentDataRowSet >= _Data.Length;
        }

        public void SetDataSets(object[][] dataSet) {
            if (_CurrentDataRowSet == 0) { //block this out if in middle of executing the generators, so nothing gets messed up
                _Data = dataSet;
            }
        }
    }
}
