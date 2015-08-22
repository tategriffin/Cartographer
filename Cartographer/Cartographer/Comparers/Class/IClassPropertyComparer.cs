using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers.Class
{
    interface IClassPropertyComparer
    {
        SimilarityRank<IPropertySymbol> Compare(IPropertySymbol sourceProperty, IPropertySymbol targetProperty);
    }
}
