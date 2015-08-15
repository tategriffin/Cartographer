using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cartographer.Filters.Compound
{
    internal class MethodHasAtLeastOneParameterAndReturnsNonPredefinedType : CompoundMethodFilter
    {
        public MethodHasAtLeastOneParameterAndReturnsNonPredefinedType()
        {
            AddFilter(new MethodHasAtLeastNParameters(1));
            AddFilter(new MethodReturnsNamedType());
        }
    }
}
