using Microsoft.CodeAnalysis;

namespace PocoEx
{
    internal class Rules
    {
        private Rules()
        { }

        /// <summary>Initialize '{0}' with the inner exception('{1}').</summary>
        public static readonly DiagnosticDescriptor PocoEx00001 = Usage(nameof(PocoEx00001));

        /// <summary>Use 'throw;' to rethrow '{0}'.</summary>
        public static readonly DiagnosticDescriptor PocoEx00002 = Usage(nameof(PocoEx00002));

        /// <summary>'bool Equals(object)' should be overridden.</summary>
        public static readonly DiagnosticDescriptor PocoEx00101 = Design(nameof(PocoEx00101));

        /// <summary>'{0}' should implement 'IEquatable&lt;{0}&gt;'.</summary>
        public static readonly DiagnosticDescriptor PocoEx00102 = Design(nameof(PocoEx00102));

        /// <summary>'bool Equals({0} {1})' should be invoked.</summary>
        public static readonly DiagnosticDescriptor PocoEx00103 = Usage(nameof(PocoEx00103));

        /// <summary>'ReferenceEquals({0}, null)' should be invoked to null-check.</summary>
        public static readonly DiagnosticDescriptor PocoEx00104 = Usage(nameof(PocoEx00104));

        /// <summary>'ReferenceEquals({0}, this)' should be invoked to check the parameter is same as the current object.</summary>
        public static readonly DiagnosticDescriptor PocoEx00105 = Usage(nameof(PocoEx00105));

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