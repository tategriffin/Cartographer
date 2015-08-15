using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer.Filters
{
    internal class MethodReturnsVoid : IMethodFilter
    {
        public virtual async Task<bool> IsSatisfiedByAsync(Document document, MethodDeclarationSyntax methodDeclaration)
        {
            return methodDeclaration.ReturnType.IsKind(SyntaxKind.VoidKeyword);
        }
    }
}
