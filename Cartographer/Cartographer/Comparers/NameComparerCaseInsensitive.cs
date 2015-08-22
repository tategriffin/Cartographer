using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cartographer.Comparers.Class;
using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers
{
    internal class NameComparerCaseInsensitive : IClassPropertyComparer
    {
        public SimilarityRank<IPropertySymbol> Compare(IPropertySymbol sourceProperty, IPropertySymbol targetProperty)
        {
            int confidence = (string.Compare(sourceProperty.Name, targetProperty.Name, StringComparison.OrdinalIgnoreCase) == 0 ? 99 : 0);

            return new SimilarityRank<IPropertySymbol>() { Confidence = confidence, Symbol = targetProperty };
        }
    }
}
