using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnitySourceGenerator.Attributes.FieldInitialization;

namespace UnitySourceGenerator
{
    [Generator]
    public class Generator : ISourceGenerator
    {
        private static class Attributes
        {
            public const int
                GET_COMPONENT_ATTRIBUTE = 0,
                GET_SINGLETON_ATTRIBUTE = 1;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxContextReceiver is SyntaxReceiver receiver))
                return;

            INamedTypeSymbol[] attributeSymbols = new[]
            {
                context.Compilation.GetTypeByMetadataName("GetComponentAttribute"),
                context.Compilation.GetTypeByMetadataName("GetSingletonAttribute")
            };

            foreach (IGrouping<INamedTypeSymbol, IFieldSymbol> group
                in receiver.Fields
                .GroupBy<IFieldSymbol, INamedTypeSymbol>(
                    f => f.ContainingType,
                    SymbolEqualityComparer.Default))
            {
                var classSource = ProcessClass(group.Key, group, attributeSymbols);
                context.AddSource($"{group.Key.Name}_Components_g.cs", SourceText.From(classSource, Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(i =>
            {
                i.AddSource("GetComponentAttribute_g.cs", Sources.s_getComponentAttribute);
                i.AddSource("GetSingletonAttribute_g.cs", Sources.s_getSingletonAttribute);
            });

            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, IEnumerable<IFieldSymbol> fields, ISymbol[] attributeSymbols)
        {
            var source = new StringBuilder($@"
public partial class {classSymbol.Name}
{{
private void InitializeFields()
{{
");
            foreach (IFieldSymbol fieldSymbol in fields)
            {
                ProcessField(source, fieldSymbol, attributeSymbols);
            }

            source.Append("}\n\n}");
            return source.ToString();
        }

        private void ProcessField(StringBuilder source, IFieldSymbol fieldSymbol, ISymbol[] attributeSymbols)
        {
            var fieldName = fieldSymbol.Name;
            ITypeSymbol fieldType = fieldSymbol.Type;

            var attributes = fieldSymbol.GetAttributes();

            var getComponentSymbol = attributeSymbols[Attributes.GET_COMPONENT_ATTRIBUTE];
            var getSingletonSymbol = attributeSymbols[Attributes.GET_SINGLETON_ATTRIBUTE];

            for (int i = 0; i < attributes.Length; i++)
            {
                var attribute = attributes[i];
                var attributeClass = attribute.AttributeClass;

                if (attributeClass.Equals(getComponentSymbol, SymbolEqualityComparer.Default))
                {
                    Append_GetComponent_Implementation(source, attribute, fieldName, fieldType);
                }
                else if (attributeClass.Equals(getSingletonSymbol, SymbolEqualityComparer.Default))
                {
                    Append_GetSingleton_Implementation(source, fieldName, fieldType);
                }
            }
        }

        private void Append_GetComponent_Implementation(StringBuilder source, AttributeData attributeData, string fieldName, ITypeSymbol fieldType)
        {
            var stringBuilder = new StringBuilder("GetComponent");

            if (attributeData.ConstructorArguments.Length > 0
                && int.TryParse(attributeData.ConstructorArguments[0].Value.ToString(), out var enumValue))
            {
                switch (enumValue)
                {
                    case 1:
                        stringBuilder.Append("InParent");
                        break;
                    case 2:
                        stringBuilder.Append("InChildren");
                        break;
                    default:
                        break;
                }
            }
            var methodType = stringBuilder.ToString();
            source.AppendLine($@"{fieldName} = {methodType}<{fieldType}>();");
        }

        private void Append_GetSingleton_Implementation(StringBuilder source, string fieldName, ITypeSymbol fieldType)
        {
            source.AppendLine($@"{fieldName} = {fieldType}.Instance;");
        }
    }
}
