using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cartographer.Mappers
{
    internal interface IMappingProbabilityCalculator
    {
        bool CanProbablyMap();
    }
}
