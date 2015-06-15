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
    internal class MapPair
    {
        public MapPair(INamedTypeSymbol sourceSymbol, string sourceName, INamedTypeSymbol targetSymbol, string targetName, TypeSyntax targetTypeSyntax)
        {
            SourceVariableName = sourceName;
            TargetVariableName = targetName;

            SourceSymbol = sourceSymbol;
            TargetSymbol = targetSymbol;

            TargetTypeSyntax = targetTypeSyntax;
        }

        public string SourceVariableName { get; set; }
        public string TargetVariableName { get; set; }

        public INamedTypeSymbol SourceSymbol { get; set; }
        public INamedTypeSymbol TargetSymbol { get; set; }

        public TypeSyntax TargetTypeSyntax { get; set; }
    }
}
