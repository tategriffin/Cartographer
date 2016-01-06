using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers.Class.Property
{
    internal abstract class CompoundClassPropertyComparer : IClassPropertyComparer
    {
        protected abstract List<IClassPropertyComparer> BuildComparers();

        public SimilarityRank<IPropertySymbol> Compare(IPropertySymbol sourceProperty, IPropertySymbol targetProperty)
        {
            List<IClassPropertyComparer> allClassPropertyComparers = BuildComparers();

            return CompoundCompare(sourceProperty, targetProperty, allClassPropertyComparers);
        }

        private SimilarityRank<IPropertySymbol> CompoundCompare(IPropertySymbol sourceProperty, IPropertySymbol targetProperty, IEnumerable<IClassPropertyComparer> comparers)
        {
            List<SimilarityRank<IPropertySymbol>> allResultRanks = new List<SimilarityRank<IPropertySymbol>>();
            foreach (var propertyComparer in comparers)
            {
                SimilarityRank<IPropertySymbol> rank = propertyComparer.Compare(sourceProperty, targetProperty);

                allResultRanks.Add(rank);
            }

            return BuildCombinedRank(targetProperty, allResultRanks);
        }

        private SimilarityRank<IPropertySymbol> BuildCombinedRank(IPropertySymbol targetProperty, List<SimilarityRank<IPropertySymbol>> individualComparerRankList)
        {
            int combinedRank = CalculateCombinedRank(individualComparerRankList);

            return new SimilarityRank<IPropertySymbol>() {Confidence = combinedRank, Symbol = targetProperty};
        }

        private int CalculateCombinedRank(List<SimilarityRank<IPropertySymbol>> individualComparerRankList)
        {
            if(individualComparerRankList == null) throw new ArgumentNullException("individualComparerRankList");
            if (individualComparerRankList.Count == 0) return 0;

            decimal combinedRank = individualComparerRankList.First().Confidence / 100;
            for (int i = 1; i < individualComparerRankList.Count; i++)  //skip first item since we've already captured it
            {
                combinedRank *= (individualComparerRankList[i].Confidence / 100);
            }

            return Convert.ToInt32((combinedRank*100));
        }

    }
}
