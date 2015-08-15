using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cartographer.Filters.Compound
{
    internal class MethodReturnsNamedType : CompoundMethodFilter
    {
        public MethodReturnsNamedType()
        {
            AddFilter(new MethodDoesNotReturnVoid());
            AddFilter(new MethodDoesNotReturnPredefinedType());
        }
    }
}
