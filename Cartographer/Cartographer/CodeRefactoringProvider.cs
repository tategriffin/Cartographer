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

            var destinationVariableName = "dest";

            // Class1 dest = new Class1();
            var destinationVariableDeclarationStatement = BuildVariableDeclarationStatement(methodDecl.ReturnType, destinationVariableName);

            //PropertyDecalartionSyntax ?
            var model = await document.GetSemanticModelAsync(cancellationToken);

            var sourceSymbol = model.GetDeclaredSymbol(firstParm);
            var destSymbol = model.GetSymbolInfo(secondParm).Symbol;


            var comp = document.Project.GetCompilationAsync(cancellationToken).Result;
            //TODO: Better way to do this instead of .ToDisplayString?
            var sourceType = comp.GetTypeByMetadataName(sourceSymbol.ToDisplayString());
            var destType = comp.GetTypeByMetadataName(destSymbol.ToDisplayString());

            var mappingStatements = BuildSourceToDestinationMappingStatements(sourceType, firstParm.Identifier.ValueText, destType, destinationVariableName);

            // return dest;
            var returnStatement = BuildReturnStatement(destinationVariableName);


            var allStatements = new List<StatementSyntax>();
            allStatements.Add(destinationVariableDeclarationStatement);
            allStatements.AddRange(mappingStatements);
            allStatements.Add(returnStatement);

            var methodBody = SyntaxFactory.Block()
                .WithStatements(SyntaxFactory.List<StatementSyntax>(allStatements));

            //root syntax tree
            var treeRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = treeRoot.ReplaceNode(methodDecl.Body, methodBody);
            var newDoc = document.WithSyntaxRoot(newRoot);

            var newSolution = newDoc.Project.Solution;
            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }

        private List<StatementSyntax> BuildSourceToDestinationMappingStatements(INamedTypeSymbol sourceSymbol, string sourceVariableName, INamedTypeSymbol destSymbol, string destVariableName)
        {
            var mappingStatements = new List<StatementSyntax>();

            //TODO: Improve the where clause selection criteria
            var srcProperties = sourceSymbol.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsWriteOnly && p.DeclaredAccessibility == Accessibility.Public).ToList();
            var destProperties = destSymbol.GetMembers().OfType<IPropertySymbol>().Where(p => !p.IsStatic && !p.IsReadOnly && p.DeclaredAccessibility == Accessibility.Public).ToList();

            //TODO: This will only work for exact matches
            foreach (var srcProperty in srcProperties)
            {
                var destProperty = destProperties.FirstOrDefault(p => p.Name == srcProperty.Name);
                if (destProperty != null)
                {
                    //add mapping statement
                    var stmt = SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(destVariableName),
                                SyntaxFactory.IdentifierName(destProperty.Name)),
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(sourceVariableName),
                                SyntaxFactory.IdentifierName(srcProperty.Name))
                            )
                        );

                    mappingStatements.Add(stmt);

                    //Only map once
                    destProperties.Remove(destProperty);
                }
            }

            return mappingStatements;
        }


        private ReturnStatementSyntax BuildReturnStatement(string destinationVariableName)
        {
            return SyntaxFactory.ReturnStatement(
                SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                SyntaxFactory.IdentifierName(destinationVariableName),
                SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.SemicolonToken, SyntaxFactory.TriviaList())
                )
                .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.CarriageReturnLineFeed));
        }

        private LocalDeclarationStatementSyntax BuildVariableDeclarationStatement(TypeSyntax destinationVariableType, string destinationVariableName)
        {
            return SyntaxFactory.LocalDeclarationStatement(BuildVariableDeclaration(destinationVariableType,
                destinationVariableName))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.SemicolonToken,
                    SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.CarriageReturnLineFeed)));
        }

        private VariableDeclarationSyntax BuildVariableDeclaration(TypeSyntax variableType, string variableName)
        {
            return SyntaxFactory.VariableDeclaration(variableType,
                    SyntaxFactory.SingletonSeparatedList<VariableDeclaratorSyntax>(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(SyntaxFactory.TriviaList(), variableName, SyntaxFactory.TriviaList(SyntaxFactory.Space)))
                            .WithInitializer(
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ObjectCreationExpression(SyntaxFactory.IdentifierName(variableType.ToString()))
                                    .WithNewKeyword(SyntaxFactory.Token(SyntaxFactory.TriviaList(SyntaxFactory.Space), SyntaxKind.NewKeyword, SyntaxFactory.TriviaList(SyntaxFactory.Space)))
                                    .WithArgumentList(SyntaxFactory.ArgumentList())
                                )
                            )
                    )
            );
        }
    }
}