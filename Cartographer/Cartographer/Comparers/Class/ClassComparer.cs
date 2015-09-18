using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cartographer.Comparers.Class.Property;
using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers.Class
{
    internal class ClassComparer : NamedTypeComparer
    {
        private readonly PropertyCountComparer ClassPropertyCountComparer;

        public ClassComparer()
        {
            ClassPropertyCountComparer = new PropertyCountComparer();
        }

        public override int CalculateSimilarityConfidenceLevel(INamedTypeSymbol sourceTypeSymbol, INamedTypeSymbol targetTypeSymbol)
        {
            if (AreTypesAbsolutelyNotComparable(sourceTypeSymbol, targetTypeSymbol)) return ZeroConfidence;

            return ClassPropertyCountComparer.CalculateSimilarityConfidenceLevel(sourceTypeSymbol, targetTypeSymbol);
        }

        protected override bool AreTypesAbsolutelyNotComparable(INamedTypeSymbol sourceTypeSymbol, INamedTypeSymbol targetTypeSymbol)
        {
            if (base.AreTypesAbsolutelyNotComparable(sourceTypeSymbol, targetTypeSymbol)) return AbsolutelyNotComparable;
            if (!sourceTypeSymbol.IsClass() || !targetTypeSymbol.IsClass()) return AbsolutelyNotComparable;

            return MightBeComparable;
        }

    }
}
