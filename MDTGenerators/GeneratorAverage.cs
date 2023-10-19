using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDTGenerators
{
    public class GeneratorAverage : GeneratorSum
    {
        public GeneratorAverage(string Name, int Interval, object[][] Data) : base(Name, Interval, Data) {
            operation = GeneratorTypes.average.ToString();
        }

        public override float CalculateResults() {            
            if ((currentDataRowSet >= 0) && !(IsDone())) { //let's be safe and double check value index
                float sumOfValues = GetSumOfValues();
                int index = currentDataRowSet; //save this, cause we want to return the calculation with the not incremented value
                currentDataRowSet++;  //increment for next iteration
                return sumOfValues / data[index].Length;
            }
            //if program had exception handling, could consider throwing exception instead
            return 0;
        }
    }
}
