using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDTGenerators
{
    public class GeneratorAverage : GeneratorSum
    {
        public GeneratorAverage(string name, int interval, object[][] data) : base(name, interval, data) {
            _Operation = GeneratorTypes.average.ToString();
        }

        public override float CalculateResults() {            
            if ((_CurrentDataRowSet >= 0) && !(IsDone())) { //let's be safe and double check value index
                float sumOfValues = GetSumOfValues();
                int index = _CurrentDataRowSet; //save this, cause we want to return the calculation with the not incremented value
                _CurrentDataRowSet++;  //increment for next iteration
                return sumOfValues / _Data[index].Length;
            }
            //if program had exception handling, could consider throwing exception instead
            return 0;
        }
    }
}
