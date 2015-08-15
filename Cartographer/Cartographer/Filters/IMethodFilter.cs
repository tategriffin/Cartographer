using System.Threading.Tasks;

namespace Cartographer.Filters
{
    public interface IMethodFilter
    {
        Task<bool> IsSatisfiedByAsync(Microsoft.CodeAnalysis.Document document, Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax methodDeclaration);
    }
}
