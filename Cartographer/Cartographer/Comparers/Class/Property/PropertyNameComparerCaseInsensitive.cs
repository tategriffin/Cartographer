using System;
using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers.Class.Property
{
    internal class PropertyNameComparerCaseInsensitive : IClassPropertyComparer
    {
        public SimilarityRank<IPropertySymbol> Compare(IPropertySymbol sourceProperty, IPropertySymbol targetProperty)
        {
            int confidence = (string.Compare(sourceProperty.Name, targetProperty.Name, StringComparison.OrdinalIgnoreCase) == 0 ? 99 : 0);

            return new SimilarityRank<IPropertySymbol>() { Confidence = confidence, Symbol = targetProperty };
        }
    }
}
