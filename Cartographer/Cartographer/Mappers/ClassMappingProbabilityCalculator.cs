using Cartographer.Comparers.Class;
using Microsoft.CodeAnalysis;

namespace Cartographer.Mappers
{
    internal class ClassMappingProbabilityCalculator : IMappingProbabilityCalculator
    {
        private readonly INamedTypeSymbol SourceTypeSymbol;
        private readonly INamedTypeSymbol TargetTypeSymbol;
        private readonly ClassComparer Comparer;

        public ClassMappingProbabilityCalculator(INamedTypeSymbol sourceTypeSymbol, INamedTypeSymbol targetTypeSymbol)
        {
            SourceTypeSymbol = sourceTypeSymbol;
            TargetTypeSymbol = targetTypeSymbol;
            Comparer = new ClassComparer();
        }

        public bool CanProbablyMap()
        {
            int confidenceLevel = Comparer.CalculateSimilarityConfidenceLevel(SourceTypeSymbol, TargetTypeSymbol);

            return (confidenceLevel > 33);
        }
    }
}
