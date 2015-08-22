﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Cartographer.Comparers.Class
{
    internal class ClassPropertyFinder
    {
        public virtual IPropertySymbol FindBestMatch(IPropertySymbol sourceProperty, IEnumerable<IPropertySymbol> targetProperties, List<IClassPropertyComparer> allComparers)
        {
            var allMatches = FindAllMatches(sourceProperty, targetProperties, allComparers);
            var topMatch = allMatches.OrderByDescending(r => r.Confidence).FirstOrDefault(r => r.Confidence > 0);

            return (topMatch != null ? topMatch.Symbol : new SimilarityRank<IPropertySymbol>().Symbol);
        }

        private List<SimilarityRank<IPropertySymbol>> FindAllMatches(IPropertySymbol sourceProperty, IEnumerable<IPropertySymbol> targetProperties, List<IClassPropertyComparer> allComparers)
        {
            var allMatches = new List<SimilarityRank<IPropertySymbol>>();

            foreach (var comparer in allComparers)
            {
                allMatches.AddRange(FindMatches(sourceProperty, targetProperties, comparer));
            }

            return allMatches;
        }

        private List<SimilarityRank<IPropertySymbol>> FindMatches(IPropertySymbol sourceProperty, IEnumerable<IPropertySymbol> targetProperties, IClassPropertyComparer comparer)
        {
            var allMatches = new List<SimilarityRank<IPropertySymbol>>();

            foreach (var targetProperty in targetProperties)
            {
                var rank = comparer.Compare(sourceProperty, targetProperty);
                if (rank.Confidence > 0)
                {
                    allMatches.Add(rank);
                }
            }

            return allMatches;
        }

    }
}
