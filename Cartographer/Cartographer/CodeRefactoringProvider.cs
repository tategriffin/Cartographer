using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cartographer.Mappers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace Cartographer
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(CartographerCodeRefactoringProvider)), Shared]
    internal class CartographerCodeRefactoringProvider : CodeRefactoringProvider
    {
        public sealed override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a method declaration node with a signature pattern we recognize.
            var methodDecl = node as MethodDeclarationSyntax;
            if (methodDecl == null) return;

            var supportedMappers = BuildMapperList();
            foreach (var mapper in supportedMappers)
            {
                if (await mapper.CanMap(context.Document, methodDecl))
                {
                    // Register mapping code action.
                    var action = CodeAction.Create(mapper.Description, c => mapper.Map(c));
                    context.RegisterRefactoring(action);

                    break;
                }
            }

        }

        private List<IMapper> BuildMapperList()
        {
            var supportedMappers = new List<IMapper>();

            supportedMappers.Add(new FirstParameterToReturnTypeClassMapper());

            return supportedMappers;
        } 

    }
}