using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Cartographer
{
    internal static class SymbolExtensions
    {
        public static bool IsEnum(this ISymbol symbol)
        {
            return (symbol as INamedTypeSymbol).IsEnum();
        }

        public static bool IsClass(this ISymbol symbol)
        {
            return (symbol as INamedTypeSymbol).IsClass();
        }

    }

    internal static class SymbolInfoExtensions
    {
        public static bool IsEnum(this SymbolInfo info)
        {
            return (info.ToNamedTypeSymbol()).IsEnum();
        }

        public static bool IsClass(this SymbolInfo info)
        {
            return (info.ToNamedTypeSymbol()).IsClass();
        }

        public static INamedTypeSymbol ToNamedTypeSymbol(this SymbolInfo info)
        {
            return info.Symbol as INamedTypeSymbol;
        }
    }

    internal static class ParameterSymbolExtensions
    {
        public static bool IsEnum(this IParameterSymbol parameterSymbol)
        {
            return (parameterSymbol.ToNamedTypeSymbol()).IsEnum();
        }

        public static bool IsClass(this IParameterSymbol parameterSymbol)
        {
            return (parameterSymbol.ToNamedTypeSymbol()).IsClass();
        }

        public static INamedTypeSymbol ToNamedTypeSymbol(this IParameterSymbol parameterSymbol)
        {
            return parameterSymbol?.Type as INamedTypeSymbol;
        }
    }

    internal static class NamedTypeSymbolExtensions
    {
        public static bool IsEnum(this INamedTypeSymbol symbol)
        {
            if (symbol == null) return false;
            if (symbol.TypeKind != TypeKind.Enum) return false;

            return true;
        }

        public static bool IsClass(this INamedTypeSymbol symbol)
        {
            if (symbol == null) return false;
            if (symbol.TypeKind == TypeKind.Enum) return false;

            return true;
        }

    }

}
