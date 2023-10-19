using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace MDTGenerators
{
    //base class for each generator
    // new generator types need to inherit from this, and supply the calc() function
    public abstract class Generator
    {
        protected string name = "";
        protected int interval = 0;
        protected string operation = ""; //initially this was not supposed to be needed, we know the type based on the class
                                         //but step 2 dictates that we have this field.
                                         // it will be initialized to match the type of class loaded
                                         //  and if the viewmodel changes it, GeneratorMain needs to correct the generator type before executing!!!
        protected object[][] data = { };
        protected int currentDataRowSet = 0;  //this is the current data set row that this generator is working on as part of generation
                                            // it should increment with each new line of data that the generator calculates

        public enum GeneratorTypes {
            sum,
            average,
            min,
            max
        }

        public string Name {
            get {
                return name;
            }
            set {
                name = value;
            }
        }

        public int Interval {
            get {
                return interval;
            }
            set {
                interval = value;
            }
        }

        public string Operation {
            get {
                return operation;
            }
            set {                
                if (operation != value) {  //we actually need to validate the operation before accepting it
                    if (ValidOperation(value)) {
                        operation = value;
                    }
                }
            }
        }

        public Generator(string Name, int Interval, object[][] Data) {
            name = Name;           
            interval = Interval > 0 ? Interval : 0; //seems like a negative interval would not make sense...
            data = Data;
            currentDataRowSet = 0;
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
            return DateTime.Now.ToString("hh:mm:ss") +" " + this.name + " " + this.CalculateResults().ToString();
        }
        
        public void Wait() {
            Thread.Sleep(interval * 1000);
        }

        public void Reset() {
            currentDataRowSet = 0;  
        }

        public bool IsDone() {
            return currentDataRowSet >= data.Length;
        }

        public void SetDataSets(object[][] DataSet) {
            if (currentDataRowSet == 0) { //block this out if in middle of executing the generators, so nothing gets messed up
                data = DataSet;
            }
        }
    }
}
