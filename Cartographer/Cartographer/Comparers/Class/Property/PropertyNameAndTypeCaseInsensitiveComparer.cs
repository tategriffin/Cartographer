using System.Collections.Generic;

namespace Cartographer.Comparers.Class.Property
{
    internal class PropertyNameAndTypeCaseInsensitiveComparer : CompoundClassPropertyComparer
    {
        protected override List<IClassPropertyComparer> BuildComparers()
        {
            return new List<IClassPropertyComparer>()
            {
                new TypeComparer(),
                new PropertyNameComparerCaseInsensitive()
            };
        }
    }
}
