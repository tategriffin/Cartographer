using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cartographer.Comparers.Class
{
    internal class PropertyTypeAndNameCaseInsensitiveComparer : CompoundClassPropertyComparer
    {
        protected override List<IClassPropertyComparer> BuildComparers()
        {
            return new List<IClassPropertyComparer>()
            {
                new TypeComparer(),
                new NameComparerCaseInsensitive()
            };
        }
    }
}
