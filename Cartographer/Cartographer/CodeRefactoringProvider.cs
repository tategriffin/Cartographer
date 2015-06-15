using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
            // TODO: Replace the following code with your own analysis, generating a CodeAction for each refactoring to offer

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            var node = root.FindNode(context.Span);

            // Only offer a refactoring if the selected node is a method declaration node.
            var methodDecl = node as MethodDeclarationSyntax;
            if (methodDecl == null
                || methodDecl.ReturnType.IsKind(SyntaxKind.VoidKeyword)
                || methodDecl.ReturnType.IsKind(SyntaxKind.PredefinedType)
                || !methodDecl.ParameterList.Parameters.Any())
            {
                return;
            }

            // For any type declaration node, create a code action to reverse the identifier text.
            var action = CodeAction.Create("Map values", c => MapFirstParameterToReturnType(context.Document, methodDecl, c));

            // Register this code action.
            context.RegisterRefactoring(action);
        }

        private async Task<Solution> MapFirstParameterToReturnType(Document document, MethodDeclarationSyntax methodDecl, CancellationToken cancellationToken)
        {
            // Get first parameter
            var firstParm = methodDecl.ParameterList.Parameters.FirstOrDefault();
            if (firstParm == null) return document.Project.Solution;

            var secondParm = methodDecl.ReturnType;
            if (secondParm == null) return document.Project.Solution;

            var model = await document.GetSemanticModelAsync(cancellationToken);
            var sourceSymbol = model.GetDeclaredSymbol(firstParm);
            var targetSymbol = model.GetSymbolInfo(secondParm).Symbol;


            var comp = document.Project.GetCompilationAsync(cancellationToken).Result;
            //TODO: Better way to do this instead of .ToDisplayString?
            var sourceType = comp.GetTypeByMetadataName(sourceSymbol.ToDisplayString());
            var targetType = comp.GetTypeByMetadataName(targetSymbol.ToDisplayString());

            //TODO: Determine target TypeSyntax from targetSymbol or targetType
            var mapInfo = new MapPair(sourceType, firstParm.Identifier.ValueText, targetType, "target", secondParm);

            var bodyBuilder = new MethodBodyBuilder();
            var methodBody = bodyBuilder.BuildMethodBody(mapInfo);

            //root syntax tree
            var treeRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = treeRoot.ReplaceNode(methodDecl.Body, methodBody);
            var newDoc = document.WithSyntaxRoot(newRoot);

            var newSolution = newDoc.Project.Solution;
            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }

    }
}