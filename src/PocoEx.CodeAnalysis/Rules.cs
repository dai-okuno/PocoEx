using Microsoft.CodeAnalysis;

namespace PocoEx.CodeAnalysis
{
    internal partial class Rules
    {
        static Rules()
        {
            PocoEx00001 = Usage(nameof(PocoEx00001));

            PocoEx00002 = Usage(nameof(PocoEx00002));

            PocoEx00101 = Design(nameof(PocoEx00101));

            PocoEx00102 = Design(nameof(PocoEx00102));

            PocoEx00103 = Usage(nameof(PocoEx00103));

            PocoEx00104 = Usage(nameof(PocoEx00104));

            PocoEx00105 = Usage(nameof(PocoEx00105));

            PocoEx00106 = Usage(nameof(PocoEx00106));
        }
        private Rules()
        { }

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
                Resources.ResourceManager.GetString(id + "Description") != null
                    ? new LocalizableResourceString(id + "Description", Resources.ResourceManager, typeof(Resources))
                    : null,
                helpLinkUri,
                customTags);
        }

    }
}