using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDTGenerators
{
    public class GeneratorSum : Generator
    {
        public GeneratorSum(string Name, int Interval, object[][] Data) : base(Name, Interval, Data) {
            operation = GeneratorTypes.sum.ToString();
        }

        public override float CalculateResults() {
            //let's be safe and double check value index
            if ((currentDataRowSet >= 0) && !(IsDone())) {
                float sumOfValues = GetSumOfValues();
                currentDataRowSet++;  //increment for next iteration
                return sumOfValues;
            }
            //if program had exception handling, could consider throwing exception instead
            return 0;
        }

        protected float GetSumOfValues() {
            float accumulator = 0;
            object[] floatArray = data[currentDataRowSet].Where(i => i is float).ToArray();
            foreach (float val in floatArray) {
                accumulator += val;
            }
            return accumulator;
        }
    }
}
