using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cartographer
{
    internal class MapItem
    {
        public string Name { get; set; }
        public TypeSyntax Syntax { get; set; }
        public INamedTypeSymbol Symbol { get; set; }
    }
}
