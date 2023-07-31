using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using UnitySourceGenerator.Utils;

namespace UnitySourceGenerator
{
    internal class SyntaxReceiver : ISyntaxContextReceiver
    {
        public List<IFieldSymbol> Fields { get; } = new List<IFieldSymbol>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is FieldDeclarationSyntax fieldDeclarationSyntax
                && fieldDeclarationSyntax.AttributeLists.Count > 0)
            {
                foreach (VariableDeclaratorSyntax variable in fieldDeclarationSyntax.Declaration.Variables)
                {
                    if (!(context.SemanticModel.GetDeclaredSymbol(variable) is IFieldSymbol fieldSymbol))
                        continue;

                    RegisterInitializerAttributes(fieldSymbol);
                }
            }
        }

        private void RegisterInitializerAttributes(IFieldSymbol fieldSymbol)
        {
            RegisterGetComponentAttribute(fieldSymbol);
            RegisterGetSingletonAttribute(fieldSymbol);
        }

        private void RegisterGetComponentAttribute(IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.ContainingType.BaseType.IsDerivedFrom("MonoBehaviour")
                && fieldSymbol.Type.BaseType.IsDerivedFrom("Component")
                && fieldSymbol.GetAttributes()
                    .Any(ad => ad.AttributeClass.ToDisplayString() == "GetComponentAttribute"))
            {
                Fields.Add(fieldSymbol);
            }
        }

        private void RegisterGetSingletonAttribute(IFieldSymbol fieldSymbol)
        {
            if (fieldSymbol.ContainingType.IsReferenceType
                && fieldSymbol.GetAttributes()
                    .Any(ad => ad.AttributeClass.ToDisplayString() == "GetSingletonAttribute"))
            {
                Fields.Add(fieldSymbol);
            }
        }
    }
}
