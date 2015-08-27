using System;
using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers.Class
{
    internal class PropertyNameComparer : IClassPropertyComparer
    {
        public SimilarityRank<IPropertySymbol> Compare(IPropertySymbol sourceProperty, IPropertySymbol targetProperty)
        {
            int confidence = (string.Compare(sourceProperty.Name, targetProperty.Name, StringComparison.Ordinal) == 0 ? 100 : 0);

            return new SimilarityRank<IPropertySymbol>() { Confidence = confidence, Symbol = targetProperty };
        }
    }
}
