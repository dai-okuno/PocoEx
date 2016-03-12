using Microsoft.CodeAnalysis;

namespace PocoEx
{
    internal class Rules
    {
        private Rules()
        { }

        public static readonly DiagnosticDescriptor PocoEx00001 = Usage(nameof(PocoEx00001));

        public static readonly DiagnosticDescriptor PocoEx00002 = Usage(nameof(PocoEx00002));

        private static DiagnosticDescriptor Usage(
            string id,
            DiagnosticSeverity defaultSeverity = DiagnosticSeverity.Warning,
            bool isEnabledByDefault = true,
            string helpLinkUri = null, params string[] customTags)

        {
            return CreateDescriptor(id, nameof(Usage), defaultSeverity, isEnabledByDefault, helpLinkUri, customTags);
        }

        private static DiagnosticDescriptor CreateDescriptor(
            string id, string category,
            DiagnosticSeverity defaultSeverity = DiagnosticSeverity.Warning,
            bool isEnabledByDefault = true,
            string helpLinkUri = null, params string[] customTags)
        {
            return new DiagnosticDescriptor(
                id,
                new LocalizableResourceString(id + "Title", Resources.ResourceManager, typeof(Resources)),
                new LocalizableResourceString(id + "MessageFormat", Resources.ResourceManager, typeof(Resources)),
                category,
                defaultSeverity,
                isEnabledByDefault,
                new LocalizableResourceString(id + "Description", Resources.ResourceManager, typeof(Resources)),
                helpLinkUri,
                customTags);
        }

    }
}