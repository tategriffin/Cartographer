using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer.Filters
{
    /// <summary>
    /// Method returns string, int, etc.
    /// </summary>
    internal class MethodReturnsPredefinedType : IMethodFilter
    {
        public virtual async Task<bool> IsSatisfiedByAsync(Document document, MethodDeclarationSyntax methodDeclaration)
        {
            return await Task.FromResult(methodDeclaration.ReturnType.IsKind(SyntaxKind.PredefinedType));
        }
    }
}
