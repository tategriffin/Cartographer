using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cartographer.Comparers
{
    internal class SimilarityRank<T> where T : class 
    {
        public SimilarityRank()
        {
            Confidence = 0;
            Symbol = null;
        }

        private int ConfidenceLevel;
        public int Confidence
        {
            get { return ConfidenceLevel; }
            set
            {
                if(value < 0 || value > 100) throw new ArgumentOutOfRangeException($"Expected: 0 to 100; Actual: {value}.");

                ConfidenceLevel = value;
            }
        }

        public T Symbol { get; set; }
    }
}
