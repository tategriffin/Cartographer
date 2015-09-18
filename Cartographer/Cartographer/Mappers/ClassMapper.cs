using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cartographer.Comparers;
using Cartographer.Comparers.Class;
using Cartographer.Comparers.Class.Property;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer.Mappers
{
    /// <summary>
    /// Base class for mapping classes
    /// </summary>
    internal abstract class ClassMapper : IRefactoringProvider
    {
        protected List<IClassPropertyComparer> ClassPropertyComparers;

        protected ClassMapper()
        {
            ClassPropertyComparers = new List<IClassPropertyComparer>
            {
                new PropertyNameAndTypeComparer(),
                new PropertyNameAndTypeCaseInsensitiveComparer()
            };
        }

        protected abstract Task<bool> CanMapAsync(Document document, MethodDeclarationSyntax methodDeclaration);

        protected abstract Task<Solution> MapAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken);

        protected bool CanProbablyMap(INamedTypeSymbol sourceTypeSymbol, INamedTypeSymbol targetTypeSymbol)
        {
            var calculator = MappingProbabilityCalculatorFactory.CreateCalculator(sourceTypeSymbol, targetTypeSymbol);
            return calculator.CanProbablyMap();
        }

        /// <summary>
        /// Sub classes must implement this method to build the actual statements within the method body.
        /// Depending on the method this may include things like instantiating a variable to return,
        /// the return statement, etc.
        /// </summary>
        protected abstract List<StatementSyntax> BuildAllBodyStatements(MapItem source, MapItem target);

        protected virtual string BuildMapperDescription(MethodDeclarationSyntax methodDeclaration, SemanticModel model, INamedTypeSymbol source, INamedTypeSymbol target)
        {
            return $"Map {source.ToMinimalDisplayString(model, methodDeclaration.SpanStart)} to {target.ToMinimalDisplayString(model, methodDeclaration.SpanStart)}";
        }

        protected BlockSyntax BuildMethodBody(MapItem source, MapItem target)
        {
            var allStatements = BuildAllBodyStatements(source, target);

            var methodBody = SyntaxFactory.Block()
                .WithStatements(SyntaxFactory.List<StatementSyntax>(allStatements));

            return methodBody;
        }

        protected List<StatementSyntax> BuildSourceToTargetMappingStatements(MapItem source, MapItem target)
        {
            return BuildSourceToTargetMappingStatements(source.Symbol, source.Name, target.Symbol, target.Name);
        }

        protected List<StatementSyntax> BuildSourceToTargetMappingStatements(INamedTypeSymbol sourceSymbol, string sourceVariableName, INamedTypeSymbol targetSymbol, string targetVariableName)
        {
            //TODO: Improve the where clause selection criteria. Should only include properties which are truely accessible from the source symbol.
            var sourceProperties = sourceSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(p => !p.IsStatic && !p.IsWriteOnly && (p.DeclaredAccessibility == Accessibility.Public || p.DeclaredAccessibility == Accessibility.Internal))
                .OrderBy(p => p.Name)
                .ToList();
            var targetProperties = targetSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(p => !p.IsStatic && !p.IsReadOnly && (p.DeclaredAccessibility == Accessibility.Public || p.DeclaredAccessibility == Accessibility.Internal))
                .OrderBy(p => p.Name)
                .ToList();

            return BuildSourceToTargetMappingStatements(sourceProperties, sourceVariableName, targetProperties, targetVariableName);
        }

        protected List<StatementSyntax> BuildSourceToTargetMappingStatements(List<IPropertySymbol> sourceProperties, string sourceVariableName, List<IPropertySymbol> targetProperties, string targetVariableName)
        {
            var mappingStatements = new List<StatementSyntax>();

            var propertyFinder = new ClassPropertyFinder();
            foreach (var srcProperty in sourceProperties)
            {
                var trgtProperty = propertyFinder.FindBestMatch(srcProperty, targetProperties, ClassPropertyComparers);
                if (trgtProperty != null)
                {
                    //add mapping statement
                    var stmt = BuildSourceToTargetMappingStatement(srcProperty, sourceVariableName, trgtProperty, targetVariableName);
                    mappingStatements.Add(stmt);

                    //Only map once
                    targetProperties.Remove(trgtProperty);
                }
            }

            return mappingStatements;
        }

        protected StatementSyntax BuildSourceToTargetMappingStatement(IPropertySymbol sourceProperty, string sourceVariableName, IPropertySymbol targetProperty, string targetVariableName)
        {
            var statement = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(targetVariableName),
                        SyntaxFactory.IdentifierName(targetProperty.Name)),
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.IdentifierName(sourceVariableName),
                        SyntaxFactory.IdentifierName(sourceProperty.Name))
                    )
                );

            return statement;
        }

        protected async Task<Solution> BuildSolutionWithNewMethodBody(Document document, MethodDeclarationSyntax methodDeclaration,
            CancellationToken cancellationToken, BlockSyntax methodBody)
        {
            //root syntax tree
            var treeRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = treeRoot.ReplaceNode(methodDeclaration.Body, methodBody);
            var newDoc = document.WithSyntaxRoot(newRoot);

            var newSolution = newDoc.Project.Solution;
            return newSolution;
        }

        public string Description { get; protected set; }

        public async Task<bool> CanRefactor(Document document, MethodDeclarationSyntax methodDeclaration)
        {
            try
            {
                return await CanMapAsync(document, methodDeclaration);
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                return false;
            }
        }

        public async Task<Solution> Refactor(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            try
            {
                return await MapAsync(document, methodDeclaration, cancellationToken);
            }
            catch (Exception ex)
            {
                string error = ex.ToString();
                throw;
            }
        }

    }
}
