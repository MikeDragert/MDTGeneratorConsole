using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDTGenerators
{
    public class GeneratorSum : Generator
    {
        public GeneratorSum(string name, int interval, object[][] data) : base(name, interval, data) {
            _Operation = GeneratorTypes.sum.ToString();
        }

        public override float CalculateResults() {
            //let's be safe and double check value index
            if ((_CurrentDataRowSet >= 0) && !(IsDone())) {
                float sumOfValues = GetSumOfValues();
                _CurrentDataRowSet++;  //increment for next iteration
                return sumOfValues;
            }
            //if program had exception handling, could consider throwing exception instead
            return 0;
        }

        protected float GetSumOfValues() {
            float accumulator = 0;
            object[] floatArray = _Data[_CurrentDataRowSet].Where(i => i is float).ToArray();
            foreach (float val in floatArray) {
                accumulator += val;
            }
            return accumulator;
        }
    }
}
