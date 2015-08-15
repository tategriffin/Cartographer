using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer.Filters
{
    internal class MethodHasAtLeastNParameters : IMethodFilter
    {
        private readonly int MinimumNumberOfParameters;

        public MethodHasAtLeastNParameters(int N)
        {
            MinimumNumberOfParameters = N;
        }

        public virtual async Task<bool> IsSatisfiedByAsync(Document document, MethodDeclarationSyntax methodDeclaration)
        {
            var numOfParameters = methodDeclaration.ParameterList?.Parameters.Count ?? 0;

            return (numOfParameters >= MinimumNumberOfParameters);
        }
    }
}
