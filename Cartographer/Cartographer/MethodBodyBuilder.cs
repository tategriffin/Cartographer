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
    internal class MethodBodyBuilder
    {
        public BlockSyntax BuildMethodBody(MapPair pair)
        {
            var allStatements = BuildAllBodyStatements(pair);

            var methodBody = SyntaxFactory.Block()
                .WithStatements(SyntaxFactory.List<StatementSyntax>(allStatements));

            return methodBody;
        }

        private List<StatementSyntax> BuildAllBodyStatements(MapPair pair)
        {
            var mapper = new MapMaker();

            var allStatements = new List<StatementSyntax>();
            
            var targetDeclaration = BuildVariableDeclarationStatement(pair.TargetTypeSyntax, pair.TargetVariableName);
            allStatements.Add(targetDeclaration);

            var mappingStatements = mapper.MapValues(pair);
            allStatements.AddRange(mappingStatements);

            var returnStatement = BuildReturnStatement(pair.TargetVariableName);
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
