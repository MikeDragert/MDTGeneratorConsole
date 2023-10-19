using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDTGenerators
{
    public class GeneratorMin : Generator
    {
        public GeneratorMin(string name, int interval, object[][] data) : base(name, interval, data) {
            _Operation = GeneratorTypes.min.ToString();
        }

        public override float CalculateResults() {
            //let's be safe and double check value index
            if ((_CurrentDataRowSet >= 0) && !(IsDone())) {
                float minValue = GetMinValue();
                _CurrentDataRowSet++;  //increment for next iteration
                return minValue;
            }
            //if program had exception handling, could consider throwing exception instead
            return 0;
        }

        private float GetMinValue() {
            float minValue = 0;
            object[] floatArray = _Data[_CurrentDataRowSet].Where(i => i is float).ToArray();
            if (floatArray.Length > 0) minValue = (float)floatArray[0]; //need this so we are locked at the initial value of 0
            foreach (float val in floatArray) {
                if (val < minValue) minValue = val;
            }
            return minValue;
        }

    }
}
