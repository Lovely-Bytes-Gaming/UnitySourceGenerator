using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitySourceGenerator.Utils
{
    internal static class Extensions
    {
        internal static bool IsDerivedFrom(this INamedTypeSymbol baseType, string targetType)
        {
            while (baseType != null)
            {
                if (baseType.Name == targetType)
                    return true;

                baseType = baseType.BaseType;
            }

            return false;
        }
    }
}
