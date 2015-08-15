using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer.Filters
{
    internal class MethodDoesNotReturnVoid : MethodReturnsVoid
    {
        public override async Task<bool> IsSatisfiedByAsync(Document document, MethodDeclarationSyntax methodDeclaration)
        {
            return !(await base.IsSatisfiedByAsync(document, methodDeclaration));
        }
    }
}
