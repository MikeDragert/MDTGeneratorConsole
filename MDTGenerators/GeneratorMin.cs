using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDTGenerators
{
    public class GeneratorMin : Generator
    {
        public GeneratorMin(string Name, int Interval, object[][] Data) : base(Name, Interval, Data) {
            operation = GeneratorTypes.min.ToString();
        }

        public override float CalculateResults() {
            //let's be safe and double check value index
            if ((currentDataRowSet >= 0) && !(IsDone())) {
                float minValue = GetMinValue();
                currentDataRowSet++;  //increment for next iteration
                return minValue;
            }
            //if program had exception handling, could consider throwing exception instead
            return 0;
        }

        private float GetMinValue() {
            float minValue = 0;
            object[] floatArray = data[currentDataRowSet].Where(i => i is float).ToArray();
            if (floatArray.Length > 0) minValue = (float)floatArray[0]; //need this so we are locked at the initial value of 0
            foreach (float val in floatArray) {
                if (val < minValue) minValue = val;
            }
            return minValue;
        }

    }
}
