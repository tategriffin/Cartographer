using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer.Mappers
{
    internal class FirstParameterToReturnTypeClassMapper : ClassMapper, IMapper
    {
        public string Description { get; private set; } = "Map first parameter to return type";

        private string BuildMapperDescription(MethodDeclarationSyntax methodDeclaration, SemanticModel model, INamedTypeSymbol source, INamedTypeSymbol target)
        {
            return $"Map {source.ToMinimalDisplayString(model, methodDeclaration.SpanStart)} to {target.ToMinimalDisplayString(model, methodDeclaration.SpanStart)}";
        }

        /// <summary>
        /// Determine whether this mapper can map values based on the method syntax and semantic model.
        /// If this method returns false, Map will not be called.
        /// </summary>
        public async Task<bool> CanMap(Document document, MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.ReturnType.IsKind(SyntaxKind.VoidKeyword)) return false;
            if (methodDeclaration.ReturnType.IsKind(SyntaxKind.PredefinedType)) return false;

            ParameterSyntax firstParameterSyntax = methodDeclaration.ParameterList?.Parameters.FirstOrDefault();
            if (firstParameterSyntax == null) return false;

            TypeSyntax returnTypeSyntax = methodDeclaration.ReturnType;
            if (returnTypeSyntax == null) return false;

            SemanticModel model = await document.GetSemanticModelAsync();

            var firstParm = model.GetDeclaredSymbol(firstParameterSyntax);
            INamedTypeSymbol firstParameterSymbol = firstParm.Type as INamedTypeSymbol;
            if (firstParameterSymbol == null) return false;
            if (firstParameterSymbol.TypeKind == TypeKind.Enum) return false;

            var returnType = model.GetSymbolInfo(returnTypeSyntax);
            INamedTypeSymbol returnTypeSymbol = returnType.Symbol as INamedTypeSymbol;
            if (returnTypeSymbol == null) return false;
            if (returnTypeSymbol.TypeKind == TypeKind.Enum) return false;

            Description = BuildMapperDescription(methodDeclaration, model, firstParameterSymbol, returnTypeSymbol);
            return true;
        }

        public async Task<Solution> Map(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            ParameterSyntax firstParameterSyntax = methodDeclaration.ParameterList.Parameters.First();
            TypeSyntax returnTypeSyntax = methodDeclaration.ReturnType;

            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken);
            var firstParm = model.GetDeclaredSymbol(firstParameterSyntax);
            INamedTypeSymbol firstParameterSymbol = firstParm.Type as INamedTypeSymbol;
            var returnType = model.GetSymbolInfo(returnTypeSyntax);
            INamedTypeSymbol returnTypeSymbol = returnType.Symbol as INamedTypeSymbol;

            var sourceItem = new MapItem() {Name = firstParameterSyntax.Identifier.ValueText, Syntax = firstParameterSyntax.Type, Symbol = firstParameterSymbol};
            var targetItem = new MapItem() {Name = "target", Syntax = returnTypeSyntax, Symbol = returnTypeSymbol};

            var methodBody = BuildMethodBody(sourceItem, targetItem);
            
            //root syntax tree
            var treeRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = treeRoot.ReplaceNode(methodDeclaration.Body, methodBody);
            var newDoc = document.WithSyntaxRoot(newRoot);
            
            var newSolution = newDoc.Project.Solution;
            
            // Return the new solution with the mapped values.
            return newSolution;
        }

        public BlockSyntax BuildMethodBody(MapItem source, MapItem target)
        {
            var allStatements = BuildAllBodyStatements(source, target);

            var methodBody = SyntaxFactory.Block()
                .WithStatements(SyntaxFactory.List<StatementSyntax>(allStatements));

            return methodBody;
        }

        private List<StatementSyntax> BuildAllBodyStatements(MapItem source, MapItem target)
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
