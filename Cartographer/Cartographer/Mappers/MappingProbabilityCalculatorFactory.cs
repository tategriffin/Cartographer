using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace Cartographer.Mappers
{
    internal static class MappingProbabilityCalculatorFactory
    {
        public static IMappingProbabilityCalculator CreateCalculator(INamedTypeSymbol sourceTypeSymbol, INamedTypeSymbol targetTypeSymbol)
        {
            if (sourceTypeSymbol == null) return new IncompatibleTypesMappingProbabilityCalculator();
            if (targetTypeSymbol == null) return new IncompatibleTypesMappingProbabilityCalculator();

            if (sourceTypeSymbol.IsClass() && targetTypeSymbol.IsClass())
            {
                return new ClassMappingProbabilityCalculator(sourceTypeSymbol, targetTypeSymbol);
            }

            return new IncompatibleTypesMappingProbabilityCalculator();
        }

    }
}
