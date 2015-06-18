using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer
{
    internal class MapMaker
    {
        public List<StatementSyntax> MapValues(MapPair pair)
        {
            return BuildSourceToDestinationMappingStatements(pair.SourceSymbol, pair.SourceVariableName, pair.TargetSymbol, pair.TargetVariableName);
        }

        private List<StatementSyntax> BuildSourceToDestinationMappingStatements(INamedTypeSymbol sourceSymbol, string sourceVariableName, INamedTypeSymbol targetSymbol, string targetVariableName)
        {
            var mappingStatements = new List<StatementSyntax>();

            //TODO: Improve the where clause selection criteria
            var sourceProperties = sourceSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(p => !p.IsStatic && !p.IsWriteOnly && (p.DeclaredAccessibility == Accessibility.Public || p.DeclaredAccessibility == Accessibility.Internal))
                .OrderBy(p => p.Name)
                .ToList();
            var targetProperties = targetSymbol.GetMembers().OfType<IPropertySymbol>()
                .Where(p => !p.IsStatic && !p.IsReadOnly && (p.DeclaredAccessibility == Accessibility.Public || p.DeclaredAccessibility == Accessibility.Internal))
                .OrderBy(p => p.Name)
                .ToList();

            //TODO: This will only work for exact matches
            foreach (var srcProperty in sourceProperties)
            {
                var trgtProperty = targetProperties.FirstOrDefault(p => p.Name == srcProperty.Name);
                if (trgtProperty != null)
                {
                    //add mapping statement
                    var stmt = SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(
                            SyntaxKind.SimpleAssignmentExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(targetVariableName),
                                SyntaxFactory.IdentifierName(trgtProperty.Name)),
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(sourceVariableName),
                                SyntaxFactory.IdentifierName(srcProperty.Name))
                            )
                        );

                    mappingStatements.Add(stmt);

                    //Only map once
                    targetProperties.Remove(trgtProperty);
                }
            }

            return mappingStatements;
        }


    }
}
