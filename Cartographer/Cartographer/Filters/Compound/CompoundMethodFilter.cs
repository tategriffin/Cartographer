using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer.Filters
{
    /// <summary>
    /// Base class for compound filters
    /// </summary>
    internal abstract class CompoundMethodFilter : IMethodFilter
    {
        private readonly List<IMethodFilter> FilterList;

        protected CompoundMethodFilter()
        {
            FilterList = new List<IMethodFilter>();
        }

        protected void AddFilter(IMethodFilter filter)
        {
            FilterList.Add(filter);
        }

        private async Task<bool> EvaluateAllFilters(Document document, MethodDeclarationSyntax methodDeclaration)
        {
            if (!FilterList.Any()) return false;

            foreach(var filter in FilterList)
            {
                if(! await filter.IsSatisfiedByAsync(document, methodDeclaration))
                {
                    return false;
                }
            }

            return true;
        }

        public Task<bool> IsSatisfiedByAsync(Document document, MethodDeclarationSyntax methodDeclaration)
        {
            return EvaluateAllFilters(document, methodDeclaration);
        }

    }
}
