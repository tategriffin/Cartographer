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
    internal class FirstParameterToReturnTypeClassMapper : IMapper
    {
        private MethodDeclarationSyntax MethodDeclaration;
        private Document CodeDocument;
        private SemanticModel CodeModel;

        private ParameterSyntax FirstParameterSyntax;
        private INamedTypeSymbol FirstParameterSymbol;

        private TypeSyntax ReturnTypeSyntax;
        private INamedTypeSymbol ReturnTypeSymbol;

        public string Description { get { return BuildMapperDescription(); } }

        private string BuildMapperDescription()
        {
            string sourceDesc = (FirstParameterSymbol == null ? "first parameter" : FirstParameterSymbol.ToMinimalDisplayString(CodeModel, MethodDeclaration.SpanStart));
            string targetDesc = (ReturnTypeSymbol == null ? "return type" : ReturnTypeSymbol.ToMinimalDisplayString(CodeModel, MethodDeclaration.SpanStart));

            return $"Map {sourceDesc} to {targetDesc}";
        }

        /// <summary>
        /// Determine whether this mapper can map values based on the method syntax and semantic model.
        /// If this method returns false, Map will not be called.
        /// </summary>
        public async Task<bool> CanMap(Document document, MethodDeclarationSyntax methodDeclaration)
        {
            return (CanMapBasedOnSyntax(methodDeclaration) && await CanMapBasedOnSemanticModel(document));
        }

        public async Task<Solution> Map(CancellationToken cancellationToken)
        {
            if (CodeDocument == null) return null;
            if (MethodDeclaration == null) return CodeDocument.Project.Solution;
            if (CodeModel == null) return CodeDocument.Project.Solution;
            if (FirstParameterSyntax == null) return CodeDocument.Project.Solution;
            if (ReturnTypeSyntax == null) return CodeDocument.Project.Solution;
            if (FirstParameterSymbol == null) return CodeDocument.Project.Solution;
            if (ReturnTypeSymbol == null) return CodeDocument.Project.Solution;

            //TODO: Determine target TypeSyntax from targetSymbol or targetType
            var mapInfo = new MapPair(FirstParameterSymbol, FirstParameterSyntax.Identifier.ValueText, ReturnTypeSymbol, "target", ReturnTypeSyntax);
            
            var bodyBuilder = new MethodBodyBuilder();
            var methodBody = bodyBuilder.BuildMethodBody(mapInfo);
            
            //root syntax tree
            var treeRoot = await CodeDocument.GetSyntaxRootAsync(cancellationToken);
            var newRoot = treeRoot.ReplaceNode(MethodDeclaration.Body, methodBody);
            var newDoc = CodeDocument.WithSyntaxRoot(newRoot);
            
            var newSolution = newDoc.Project.Solution;
            
            // Return the new solution with the mapped values.
            return newSolution;
        }

        /// <summary>
        /// Perform an initial check to see if the syntax matches what we expect.
        /// If this method returns false, CanMap will not be called.
        /// </summary>
        private bool CanMapBasedOnSyntax(MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration == null) return false;
            if (methodDeclaration.ReturnType.IsKind(SyntaxKind.VoidKeyword)) return false;
            if (methodDeclaration.ReturnType.IsKind(SyntaxKind.PredefinedType)) return false;
            if (methodDeclaration.ParameterList == null) return false;

            FirstParameterSyntax = methodDeclaration.ParameterList?.Parameters.FirstOrDefault();
            if (FirstParameterSyntax == null) return false;

            ReturnTypeSyntax = methodDeclaration.ReturnType;
            if (ReturnTypeSyntax == null) return false;

            MethodDeclaration = methodDeclaration;
            return true;
        }

        private async Task<bool> CanMapBasedOnSemanticModel(Document document)
        {
            if (FirstParameterSyntax == null) return false;
            if (ReturnTypeSyntax == null) return false;

            CodeDocument = document;
            CodeModel = await document.GetSemanticModelAsync();

            var firstParm = CodeModel.GetDeclaredSymbol(FirstParameterSyntax);
            FirstParameterSymbol = firstParm.Type as INamedTypeSymbol;
            if (FirstParameterSymbol == null) return false;
            if (FirstParameterSymbol.TypeKind == TypeKind.Enum) return false;

            var returnType = CodeModel.GetSymbolInfo(ReturnTypeSyntax);
            ReturnTypeSymbol = returnType.Symbol as INamedTypeSymbol;
            if (ReturnTypeSymbol == null) return false;
            if (ReturnTypeSymbol.TypeKind == TypeKind.Enum) return false;

            return true;
        }

    }
}
