using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers
{
    internal abstract class NamedTypeComparer
    {
        protected const bool AbsolutelyNotComparable = true;
        protected const bool MightBeComparable = false;
        protected const int ZeroConfidence = 0;

        public abstract int CalculateSimilarityConfidenceLevel(INamedTypeSymbol sourceTypeSymbol, INamedTypeSymbol targetTypeSymbol);

        protected virtual bool AreTypesAbsolutelyNotComparable(INamedTypeSymbol sourceTypeSymbol, INamedTypeSymbol targetTypeSymbol)
        {
            if (sourceTypeSymbol == null || targetTypeSymbol == null) return AbsolutelyNotComparable;

            return MightBeComparable;
        }

    }
}
