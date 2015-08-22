using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers.Class
{
    internal abstract class CompoundClassPropertyComparer : IClassPropertyComparer
    {
        protected abstract List<IClassPropertyComparer> BuildComparers();

        public SimilarityRank<IPropertySymbol> Compare(IPropertySymbol sourceProperty, IPropertySymbol targetProperty)
        {
            List<IClassPropertyComparer> allClassPropertyComparers = BuildComparers();
            List<SimilarityRank<IPropertySymbol>> allResultRanks = CompareAll(sourceProperty, targetProperty, allClassPropertyComparers);

            var bestMatch = FindProbableMatch(allResultRanks);
            return (bestMatch ?? new SimilarityRank<IPropertySymbol>() { Confidence = 0 });

        }

        private List<SimilarityRank<IPropertySymbol>> CompareAll(IPropertySymbol sourceProperty, IPropertySymbol targetProperty, IEnumerable<IClassPropertyComparer> comparers)
        {
            List<SimilarityRank<IPropertySymbol>> allResultRanks = new List<SimilarityRank<IPropertySymbol>>();
            foreach (var propertyComparer in comparers)
            {
                SimilarityRank<IPropertySymbol> rank = propertyComparer.Compare(sourceProperty, targetProperty);

                allResultRanks.Add(rank);

                if(rank.Confidence == 100) break;
            }

            return allResultRanks;
        }

        private SimilarityRank<IPropertySymbol> FindProbableMatch(IEnumerable<SimilarityRank<IPropertySymbol>> allRanks)
        {
            return allRanks
                .OrderByDescending(r => r.Confidence)
                .FirstOrDefault(r => r.Confidence > 0);
        }
    }
}
