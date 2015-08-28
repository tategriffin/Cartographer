using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers.Class.Property
{
    interface IClassPropertyComparer
    {
        SimilarityRank<IPropertySymbol> Compare(IPropertySymbol sourceProperty, IPropertySymbol targetProperty);
    }
}
