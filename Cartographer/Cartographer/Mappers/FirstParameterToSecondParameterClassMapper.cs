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
    class FirstParameterToSecondParameterClassMapper : ClassMapper
    {
        public FirstParameterToSecondParameterClassMapper()
        {
            Description = "MapAsync first parameter to second parameter";
        }

        private string BuildMapperDescription(MethodDeclarationSyntax methodDeclaration, SemanticModel model, IParameterSymbol source, IParameterSymbol target)
        {
            return BuildMapperDescription(methodDeclaration, model, source.ToNamedTypeSymbol(), target.ToNamedTypeSymbol());
        }

        /// <summary>
        /// Determine whether this mapper can map values based on the method syntax and semantic model.
        /// If this method returns false, MapAsync will not be called.
        /// </summary>
        protected override async Task<bool> CanMapAsync(Document document, MethodDeclarationSyntax methodDeclaration)
        {
            if (!await new MethodHasAtLeastTwoParametersAndReturnsVoid().IsSatisfiedByAsync(document, methodDeclaration)) return false;

            ParameterSyntax firstParameterSyntax = methodDeclaration.ParameterList.Parameters[0];
            ParameterSyntax secondParameterSyntax = methodDeclaration.ParameterList.Parameters[1];

            SemanticModel model = await document.GetSemanticModelAsync();
            var firstParm = model.GetDeclaredSymbol(firstParameterSyntax);
            var secondParm = model.GetDeclaredSymbol(secondParameterSyntax);

            if (firstParm.IsClass() && secondParm.IsClass())
            {
                Description = BuildMapperDescription(methodDeclaration, model, firstParm, secondParm);
                return true;
            }

            return false;
        }

        protected override async Task<Solution> MapAsync(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            ParameterSyntax firstParameterSyntax = methodDeclaration.ParameterList.Parameters[0];
            ParameterSyntax secondParameterSyntax = methodDeclaration.ParameterList.Parameters[1];

            SemanticModel model = await document.GetSemanticModelAsync(cancellationToken);
            INamedTypeSymbol firstParameter = model.GetDeclaredSymbol(firstParameterSyntax).ToNamedTypeSymbol();
            INamedTypeSymbol secondParameter = model.GetDeclaredSymbol(secondParameterSyntax).ToNamedTypeSymbol();

            var sourceItem = new MapItem() { Name = firstParameterSyntax.Identifier.ValueText, Syntax = firstParameterSyntax.Type, Symbol = firstParameter };
            var targetItem = new MapItem() { Name = secondParameterSyntax.Identifier.ValueText, Syntax = secondParameterSyntax.Type, Symbol = secondParameter };

            var methodBody = BuildMethodBody(sourceItem, targetItem);
            var newSolution = await BuildSolutionWithNewMethodBody(document, methodDeclaration, cancellationToken, methodBody);

            // Return the new solution with the method containing the mapping code.
            return newSolution;
        }

        protected override List<StatementSyntax> BuildAllBodyStatements(MapItem source, MapItem target)
        {
            var allStatements = new List<StatementSyntax>();

            var mappingStatements = BuildSourceToTargetMappingStatements(source, target);
            allStatements.AddRange(mappingStatements);

            return allStatements;
        }

    }
}
