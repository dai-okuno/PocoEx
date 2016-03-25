using Microsoft.CodeAnalysis;

namespace PocoEx.CodeAnalysis
{
    partial class Rules
    {

        /// <summary>Use 'throw;' to rethrow '{0}'.</summary>
        public static readonly DiagnosticDescriptor PocoEx00001;

        /// <summary>Initialize '{0}' with the inner exception('{1}').</summary>
        public static readonly DiagnosticDescriptor PocoEx00002;

        /// <summary>'Equals(object)' should be overridden.</summary>
        public static readonly DiagnosticDescriptor PocoEx00101;

        /// <summary>'{0}' should implement 'IEquatable<{0}>'.</summary>
        public static readonly DiagnosticDescriptor PocoEx00102;

        /// <summary>'Equals(object)' should invoke {0}.</summary>
        public static readonly DiagnosticDescriptor PocoEx00103;

        /// <summary>'ReferenceEquals({0}, null)' should be invoked to check for null reference.</summary>
        public static readonly DiagnosticDescriptor PocoEx00104;

        /// <summary>'ReferenceEquals({0}, this)' should be invoked to check for reference equality.</summary>
        public static readonly DiagnosticDescriptor PocoEx00105;

        /// <summary>'Equals({0})' should check for the equality of '{1}'.</summary>
        public static readonly DiagnosticDescriptor PocoEx00106;

        /// <summary>Create a <see cref="DiagnosticDescriptor"/> belongs "Usage" category.</summary>
        /// <param name="id"></param>
        /// <param name="defaultSeverity">Default severity of the diagnostic. Default is <see cref="DiagnosticSeverity.Warning"/></param>
        /// <param name="isEnabledByDefault">True if the diagnostic is enabled by default. Default is true.</param>
        /// <param name="helpLinkUri">An optional hyperlink that provides a more detailed description regarding the diagnostic.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic.</param>
        /// <returns>a <see cref="DiagnosticDescriptor"/> belongs "Usage" category.</returns>
        private static DiagnosticDescriptor Usage(
            string id,
            DiagnosticSeverity defaultSeverity = DiagnosticSeverity.Warning,
            bool isEnabledByDefault = true,
            string helpLinkUri = null, params string[] customTags)

        {
            return CreateDescriptor(id, nameof(Usage), defaultSeverity, isEnabledByDefault, helpLinkUri, customTags);
        }

        /// <summary>Create a <see cref="DiagnosticDescriptor"/> belongs "Design" category.</summary>
        /// <param name="id"></param>
        /// <param name="defaultSeverity">Default severity of the diagnostic. Default is <see cref="DiagnosticSeverity.Warning"/></param>
        /// <param name="isEnabledByDefault">True if the diagnostic is enabled by default. Default is true.</param>
        /// <param name="helpLinkUri">An optional hyperlink that provides a more detailed description regarding the diagnostic.</param>
        /// <param name="customTags">Optional custom tags for the diagnostic.</param>
        /// <returns>a <see cref="DiagnosticDescriptor"/> belongs "Design" category.</returns>
        private static DiagnosticDescriptor Design(
            string id,
            DiagnosticSeverity defaultSeverity = DiagnosticSeverity.Warning,
            bool isEnabledByDefault = true,
            string helpLinkUri = null, params string[] customTags)

        {
            return CreateDescriptor(id, nameof(Design), defaultSeverity, isEnabledByDefault, helpLinkUri, customTags);
        }
    }
}