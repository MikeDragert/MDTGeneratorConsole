using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDTGenerators
{
    public class GeneratorMax : Generator
    {
        public GeneratorMax(string name, int interval, object[][] data) : base(name, interval, data) {
            _Operation = GeneratorTypes.max.ToString();
        }

        public override float CalculateResults() {            
            if ((_CurrentDataRowSet >= 0) && !(IsDone())) { //let's be safe and double check value index
                float maxValue = GetMaxValue();
                _CurrentDataRowSet++;  //increment for next iteration
                return maxValue;
            }
            //if program had exception handling, could consider throwing exception instead
            return 0;
        }

        private float GetMaxValue() {
            float maxValue = 0;
            object[] floatArray = _Data[_CurrentDataRowSet].Where(i => i is float).ToArray();
            if (floatArray.Length > 0) maxValue = (float)floatArray[0]; //this line is here in case there are negative numbers!!
            foreach (float val in floatArray) {
                if (val > maxValue) maxValue = val;
            }
            return maxValue;
        }
    }
}
