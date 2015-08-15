using System;
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
            try
            {
                var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

                // Find the node at the selection.
                var node = root.FindNode(context.Span);

                // Only offer a refactoring if the selected node is a method declaration node.
                var methodDecl = node as MethodDeclarationSyntax;
                if (methodDecl == null) return;

                // Only offer a refactoring if the method declaration node matches a signature pattern we recognize.
                var refactoringProviderList = BuildRefactoringProviderList();
                foreach (var mapper in refactoringProviderList)
                {
                    if (await mapper.CanRefactor(context.Document, methodDecl))
                    {
                        // Register mapping code action.
                        var action = CodeAction.Create(mapper.Description, c => mapper.Refactor(context.Document, methodDecl, c));
                        context.RegisterRefactoring(action);

                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
            }
        }

        private List<IRefactoringProvider> BuildRefactoringProviderList()
        {
            var supportedMappers = new List<IRefactoringProvider>();

            supportedMappers.Add(new FirstParameterToReturnTypeClassMapper());
            supportedMappers.Add(new FirstParameterToSecondParameterClassMapper());

            return supportedMappers;
        } 

    }
}