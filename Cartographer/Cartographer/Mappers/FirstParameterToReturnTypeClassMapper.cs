using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cartographer.Filters.Compound;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer.Mappers
{
    internal class FirstParameterToReturnTypeClassMapper : ClassMapper
    {
        public FirstParameterToReturnTypeClassMapper()
        {
            Description = "Map first parameter to return type";
        }

        private string BuildMapperDescription(MethodDeclarationSyntax methodDeclaration, SemanticModel model, IParameterSymbol source, SymbolInfo target)
        {
            return BuildMapperDescription(methodDeclaration, model, source.ToNamedTypeSymbol(), target.ToNamedTypeSymbol());
        }

        /// <summary>
        /// Determine whether this mapper can map values based on the method syntax and semantic model.
        /// If this method returns false, MapAsync will not be called.
        /// </summary>
        protected override async Task<bool> CanMapAsync(Document document, MethodDeclarationSyntax methodDeclaration)
        {
            if (! await new MethodHasAtLeastOneParameterAndReturnsNonPredefinedType().IsSatisfiedByAsync(document, methodDeclaration)) return false;

            ParameterSyntax firstParameterSyntax = methodDeclaration.ParameterList.Parameters.First();
            TypeSyntax returnTypeSyntax = methodDeclaration.ReturnType;

            SemanticModel model = await document.GetSemanticModelAsync();
            var firstParm = model.GetDeclaredSymbol(firstParameterSyntax);
            var returnType = model.GetSymbolInfo(returnTypeSyntax);

            if (firstParm.IsClass() && returnType.IsClass())
            {
                Description = BuildMapperDescription(methodDeclaration, model, firstParm, returnType);
                return true;
            }

            return false;
        }

        protected override async Task<Solution> MapAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            ParameterSyntax firstParameterSyntax = methodDeclaration.ParameterList.Parameters.First();
            TypeSyntax returnTypeSyntax = methodDeclaration.ReturnType;

            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken);
            INamedTypeSymbol firstParameter = model.GetDeclaredSymbol(firstParameterSyntax).ToNamedTypeSymbol();
            INamedTypeSymbol returnTypeSymbol = model.GetSymbolInfo(returnTypeSyntax).ToNamedTypeSymbol();

            var sourceItem = new MapItem() {Name = firstParameterSyntax.Identifier.ValueText, Syntax = firstParameterSyntax.Type, Symbol = firstParameter};
            var targetItem = new MapItem() {Name = "target", Syntax = returnTypeSyntax, Symbol = returnTypeSymbol};

            var methodBody = BuildMethodBody(sourceItem, targetItem);
            var newSolution = await BuildSolutionWithNewMethodBody(document, methodDeclaration, cancellationToken, methodBody);

            // Return the new solution with the method containing the mapping code.
            return newSolution;
        }

        protected override List<StatementSyntax> BuildAllBodyStatements(MapItem source, MapItem target)
        {
            var allStatements = new List<StatementSyntax>();

            var targetDeclaration = BuildVariableDeclarationStatement(target.Syntax, target.Name);
            allStatements.Add(targetDeclaration);

            var mappingStatements = BuildSourceToTargetMappingStatements(source, target);
            allStatements.AddRange(mappingStatements);

            var returnStatement = BuildReturnStatement(target.Name);
            allStatements.Add(returnStatement);
            return allStatements;
        }

        // return target;
        private ReturnStatementSyntax BuildReturnStatement(string targetVariableName)
        {
            return SyntaxFactory.ReturnStatement(
                SyntaxFactory.Token(SyntaxKind.ReturnKeyword),
                SyntaxFactory.IdentifierName(targetVariableName),
                SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.SemicolonToken, SyntaxFactory.TriviaList())
                )
                .WithLeadingTrivia(SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.CarriageReturnLineFeed));
        }

        // Class1 target = new Class1();
        private LocalDeclarationStatementSyntax BuildVariableDeclarationStatement(TypeSyntax targetVariableType, string targetVariableName)
        {
            return SyntaxFactory.LocalDeclarationStatement(BuildVariableDeclaration(targetVariableType,
                targetVariableName))
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxFactory.TriviaList(), SyntaxKind.SemicolonToken,
                    SyntaxFactory.TriviaList(SyntaxFactory.CarriageReturnLineFeed, SyntaxFactory.CarriageReturnLineFeed)));
        }

        // The "target = new Class1()" portion of "Class1 target = new Class1();"
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
