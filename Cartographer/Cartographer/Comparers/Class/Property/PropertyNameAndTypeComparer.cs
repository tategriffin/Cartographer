using System.Collections.Generic;

namespace Cartographer.Comparers.Class.Property
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
