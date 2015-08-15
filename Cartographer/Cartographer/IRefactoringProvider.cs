using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer
{
    internal interface IRefactoringProvider
    {
        string Description { get; }

        Task<bool> CanRefactor(Document document, MethodDeclarationSyntax methodDeclaration);

        Task<Solution> Refactor(Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken);
    }
}
