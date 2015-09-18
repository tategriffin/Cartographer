using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers.Class.Property
{
    internal class PropertyCountComparer : NamedTypeComparer
    {
        public override int CalculateSimilarityConfidenceLevel(INamedTypeSymbol sourceTypeSymbol, INamedTypeSymbol targetTypeSymbol)
        {
            if (AreTypesAbsolutelyNotComparable(sourceTypeSymbol, targetTypeSymbol)) return 0;

            return CalculatePropertyCountSimilarity(sourceTypeSymbol, targetTypeSymbol);
        }

        private int CalculatePropertyCountSimilarity(INamedTypeSymbol sourceTypeSymbol, INamedTypeSymbol targetTypeSymbol)
        {
            int sourceTypePropertyCount = CountAccessibleProperties(sourceTypeSymbol);
            int targetTypePropertyCount = CountAccessibleProperties(targetTypeSymbol);

            return Convert.ToInt32(CalculatePropertyCountSimilarityPercent(sourceTypePropertyCount, targetTypePropertyCount));
        }

        private double CalculatePropertyCountSimilarityPercent(int sourceTypePropertyCount, int targetTypePropertyCount)
        {
            if (sourceTypePropertyCount == 0 && targetTypePropertyCount == 0) return 0;

            var propertyCounts = BuildDivisionTuple(sourceTypePropertyCount, targetTypePropertyCount);
            //TODO: Improve algorithm. Example: (3 properties, allow 1% match; 20 properties, require 99% match)
            double samePropertyPercent = ((double)(propertyCounts.Item1) / propertyCounts.Item2);

            return samePropertyPercent * 100.0;
        }

        private int CountAccessibleProperties(INamedTypeSymbol namedTypeSymbol)
        {
            //Determining which properties are accessible is a difficult problem.
            //Do something simple for now.
            return namedTypeSymbol.GetMembers()
                .OfType<IPropertySymbol>()
                .Count(p => !p.IsStatic && !p.IsReadOnly && !p.IsWriteOnly && (p.DeclaredAccessibility == Accessibility.Public));
        }

        private Tuple<int, int> BuildDivisionTuple(int first, int second)
        {
            if (first == 0 && second == 0) throw new ArgumentException("Both arguments are 0. Cannot divide by 0.");
            if (first == 0) return new Tuple<int, int>(first, second);
            if (second == 0) return new Tuple<int, int>(second, first);

            if (first > second)
            {
                return new Tuple<int, int>(second, first);
            }

            return new Tuple<int, int>(first, second);
        }

    }
}
