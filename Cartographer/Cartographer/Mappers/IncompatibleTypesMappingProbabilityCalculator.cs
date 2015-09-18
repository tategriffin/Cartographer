using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cartographer.Mappers
{
    internal class IncompatibleTypesMappingProbabilityCalculator : IMappingProbabilityCalculator
    {
        public bool CanProbablyMap()
        {
            return false;
        }
    }
}
