using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer
{
    internal interface IMapper
    {
        string Description { get; }

        /// <summary>
        /// Determine whether this mapper can map values based on the method syntax and semantic model.
        /// If this method returns false, Map will not be called.
        /// </summary>
        Task<bool> CanMap(Document document, MethodDeclarationSyntax methodDeclaration);

        Task<Solution> Map(CancellationToken cancellationToken);
    }
}