using Microsoft.CodeAnalysis;

namespace PocoEx
{
    internal class Rules
    {
        private Rules()
        { }

        public static readonly DiagnosticDescriptor PocoEx00001 = Usage(nameof(PocoEx00001));

        public static readonly DiagnosticDescriptor PocoEx00002 = Usage(nameof(PocoEx00002));

        /// <summary>'bool Equals(T)' is declared, but 'bool Equals(object)' is not overridden.</summary>
        public static readonly DiagnosticDescriptor PocoEx00101 = Design(nameof(PocoEx00101));

        /// <summary>A structure type should implement 'IEquatable&lt;T&gt;'</summary>
        public static readonly DiagnosticDescriptor PocoEx00102 = Design(nameof(PocoEx00102));

        /// <summary>Invoke 'ReferenceEquals(parameter, null)' to null-check.</summary>
        public static readonly DiagnosticDescriptor PocoEx00104 = Usage(nameof(PocoEx00104));

        /// <summary>Invoke 'ReferenceEquals(parameter, this)' to check reference-equality.</summary>
        public static readonly DiagnosticDescriptor PocoEx00105 = Usage(nameof(PocoEx00105));

        /// <summary>Invoke all of 'bool Equals(T)'.</summary>
        public static readonly DiagnosticDescriptor PocoEx00103 = Usage(nameof(PocoEx00103));

        /// <summary>Create a <see cref="DiagnosticDescriptor"/> belongs 'Design' category.</summary>
        /// <param name="id"></param>
        /// <param name="defaultSeverity">Default severity of the diagnostic. Default is <see cref="DiagnosticSeverity.Warning"/></param>
        /// <param name="isEnabledByDefault">True if the diagnostic is enabled by default. Default is true.</param>
        /// <param name="helpLinkUri">An optional hyperlink that provides a more detailed description regarding the diagnostic.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic.</param>
        /// <returns>a <see cref="DiagnosticDescriptor"/> belongs 'Design' category.</returns>
        private static DiagnosticDescriptor Design(
            string id,
            DiagnosticSeverity defaultSeverity = DiagnosticSeverity.Warning,
            bool isEnabledByDefault = true,
            string helpLinkUri = null, params string[] customTags)

        {
            return CreateDescriptor(id, nameof(Design), defaultSeverity, isEnabledByDefault, helpLinkUri, customTags);
        }

        /// <summary>Create a <see cref="DiagnosticDescriptor"/> belongs 'Naming' category.</summary>
        /// <param name="id"></param>
        /// <param name="defaultSeverity">Default severity of the diagnostic. Default is <see cref="DiagnosticSeverity.Warning"/></param>
        /// <param name="isEnabledByDefault">True if the diagnostic is enabled by default. Default is true.</param>
        /// <param name="helpLinkUri">An optional hyperlink that provides a more detailed description regarding the diagnostic.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic.</param>
        /// <returns>a <see cref="DiagnosticDescriptor"/> belongs 'Naming' category.</returns>
        private static DiagnosticDescriptor Naming(
            string id,
            DiagnosticSeverity defaultSeverity = DiagnosticSeverity.Warning,
            bool isEnabledByDefault = true,
            string helpLinkUri = null, params string[] customTags)

        {
            return CreateDescriptor(id, nameof(Naming), defaultSeverity, isEnabledByDefault, helpLinkUri, customTags);
        }

        /// <summary>Create a <see cref="DiagnosticDescriptor"/> belongs 'Usage' category.</summary>
        /// <param name="id"></param>
        /// <param name="defaultSeverity">Default severity of the diagnostic. Default is <see cref="DiagnosticSeverity.Warning"/></param>
        /// <param name="isEnabledByDefault">True if the diagnostic is enabled by default. Default is true.</param>
        /// <param name="helpLinkUri">An optional hyperlink that provides a more detailed description regarding the diagnostic.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic.</param>
        /// <returns>a <see cref="DiagnosticDescriptor"/> belongs 'Usage' category.</returns>
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
                Resources.ResourceManager.GetString(id + "Description") == null
                    ? new LocalizableResourceString(id + "Description", Resources.ResourceManager, typeof(Resources))
                    : null,
                helpLinkUri,
                customTags);
        }

    }
}