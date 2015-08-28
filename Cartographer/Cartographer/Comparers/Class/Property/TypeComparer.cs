using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers.Class.Property
{
    internal class TypeComparer : IClassPropertyComparer
    {
        public SimilarityRank<IPropertySymbol> Compare(IPropertySymbol sourceProperty, IPropertySymbol targetProperty)
        {
            //TODO: check for subclasses
            bool sameType = sourceProperty.Type.Equals(targetProperty.Type);
            int confidence = (sameType ? 100 : 0);

            return new SimilarityRank<IPropertySymbol>() { Confidence = confidence, Symbol = targetProperty };
        }
    }
}
