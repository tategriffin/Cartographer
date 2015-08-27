using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers.Class
{
    internal class PropertyNameAndTypeComparer : CompoundClassPropertyComparer
    {
        protected override List<IClassPropertyComparer> BuildComparers()
        {
            return new List<IClassPropertyComparer>()
            {
                new TypeComparer(),
                new PropertyNameComparer(),
            };
        }
    }
}
