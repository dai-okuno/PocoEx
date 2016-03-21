using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PocoEx
{
    partial class Utils
    {

        /// <summary>Gets the declaration syntaxes of <see cref="INamedTypeSymbol"/>.</summary>
        /// <param name="symbol">The symbol to get declarations.</param>
        public static Task<IEnumerable<TypeDeclarationSyntax>> GetDeclarationSyntaxAsync(this INamedTypeSymbol symbol, CancellationToken cancellationToken = default(CancellationToken))
            => GetDeclarationSyntaxAsync<TypeDeclarationSyntax>(symbol, cancellationToken);

        /// <summary>Gets the declaration syntaxes of <see cref="IMethodSymbol"/>.</summary>
        /// <param name="symbol">The symbol to get declarations.</param>
        public static Task<IEnumerable<MethodDeclarationSyntax>> GetDeclarationSyntaxAsync(this IMethodSymbol symbol, CancellationToken cancellationToken = default(CancellationToken))
            => GetDeclarationSyntaxAsync<MethodDeclarationSyntax>(symbol, cancellationToken);

        /// <summary>Gets the declaration syntaxes of <see cref="IParameterSymbol"/>.</summary>
        /// <param name="symbol">The symbol to get declarations.</param>
        public static Task<IEnumerable<ParameterSyntax>> GetDeclarationSyntaxAsync(this IParameterSymbol symbol, CancellationToken cancellationToken = default(CancellationToken))
            => GetDeclarationSyntaxAsync<ParameterSyntax>(symbol, cancellationToken);

        /// <summary>Gets the declaration syntaxes of <see cref="IPropertySymbol"/>.</summary>
        /// <param name="symbol">The symbol to get declarations.</param>
        public static Task<IEnumerable<PropertyDeclarationSyntax>> GetDeclarationSyntaxAsync(this IPropertySymbol symbol, CancellationToken cancellationToken = default(CancellationToken))
            => GetDeclarationSyntaxAsync<PropertyDeclarationSyntax>(symbol, cancellationToken);

        /// <summary>Gets the declaration syntaxes of <see cref="IEventSymbol"/>.</summary>
        /// <param name="symbol">The symbol to get declarations.</param>
        public static Task<IEnumerable<EventDeclarationSyntax>> GetDeclarationSyntaxAsync(this IEventSymbol symbol, CancellationToken cancellationToken = default(CancellationToken))
            => GetDeclarationSyntaxAsync<EventDeclarationSyntax>(symbol, cancellationToken);

        /// <summary>Gets the location of <see cref="TypeDeclarationSyntax.Identifier"/>.</summary>
        /// <param name="nodes">Nodes to get the location of the identifier.</param>
        /// <returns>The locations of the identifier.</returns>
        public static IEnumerable<Location> GetIdentifierLocations(this IEnumerable<TypeDeclarationSyntax> nodes)
			=> nodes.Select(node => node.Identifier.GetLocation());

        /// <summary>Gets the location of <see cref="MethodDeclarationSyntax.Identifier"/>.</summary>
        /// <param name="nodes">Nodes to get the location of the identifier.</param>
        /// <returns>The locations of the identifier.</returns>
        public static IEnumerable<Location> GetIdentifierLocations(this IEnumerable<MethodDeclarationSyntax> nodes)
			=> nodes.Select(node => node.Identifier.GetLocation());

        /// <summary>Gets the location of <see cref="PropertyDeclarationSyntax.Identifier"/>.</summary>
        /// <param name="nodes">Nodes to get the location of the identifier.</param>
        /// <returns>The locations of the identifier.</returns>
        public static IEnumerable<Location> GetIdentifierLocations(this IEnumerable<PropertyDeclarationSyntax> nodes)
			=> nodes.Select(node => node.Identifier.GetLocation());

        /// <summary>Gets the location of <see cref="EventDeclarationSyntax.Identifier"/>.</summary>
        /// <param name="nodes">Nodes to get the location of the identifier.</param>
        /// <returns>The locations of the identifier.</returns>
        public static IEnumerable<Location> GetIdentifierLocations(this IEnumerable<EventDeclarationSyntax> nodes)
			=> nodes.Select(node => node.Identifier.GetLocation());

        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="node">A syntax node related with the diagnostic.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic<TSyntax>(this SymbolAnalysisContext context, DiagnosticDescriptor descriptor, TSyntax node, params object[] messageArgs)
            where TSyntax : SyntaxNode
			=> context.ReportDiagnostic(Diagnostic.Create(descriptor, node.GetLocation(), messageArgs));

        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="nodes">Syntax nodes related with the diagnostic.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic<TSyntax>(this SymbolAnalysisContext context, DiagnosticDescriptor descriptor, IEnumerable<TSyntax> nodes, params object[] messageArgs)
            where TSyntax : SyntaxNode
			=> ReportDiagnostic(context, descriptor, nodes.Select(node => node.GetLocation()), messageArgs);

        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="locations">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic(this SymbolAnalysisContext context, DiagnosticDescriptor descriptor, IEnumerable<Location> locations, params object[] messageArgs)
        {
			var location = locations.FirstOrDefault() ?? Location.None;
            context.ReportDiagnostic(Diagnostic.Create(descriptor, location, locations.Skip(1), messageArgs));
        }

        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="locations">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic(this SymbolAnalysisContext context, DiagnosticDescriptor descriptor, ImmutableArray<Location> locations, params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(descriptor, locations[0], locations.Skip(1), messageArgs));
        }
        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="node">A syntax node related with the diagnostic.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic<TSyntax>(this CodeBlockAnalysisContext context, DiagnosticDescriptor descriptor, TSyntax node, params object[] messageArgs)
            where TSyntax : SyntaxNode
			=> context.ReportDiagnostic(Diagnostic.Create(descriptor, node.GetLocation(), messageArgs));

        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="nodes">Syntax nodes related with the diagnostic.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic<TSyntax>(this CodeBlockAnalysisContext context, DiagnosticDescriptor descriptor, IEnumerable<TSyntax> nodes, params object[] messageArgs)
            where TSyntax : SyntaxNode
			=> ReportDiagnostic(context, descriptor, nodes.Select(node => node.GetLocation()), messageArgs);

        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="locations">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic(this CodeBlockAnalysisContext context, DiagnosticDescriptor descriptor, IEnumerable<Location> locations, params object[] messageArgs)
        {
			var location = locations.FirstOrDefault() ?? Location.None;
            context.ReportDiagnostic(Diagnostic.Create(descriptor, location, locations.Skip(1), messageArgs));
        }

        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="locations">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic(this CodeBlockAnalysisContext context, DiagnosticDescriptor descriptor, ImmutableArray<Location> locations, params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(descriptor, locations[0], locations.Skip(1), messageArgs));
        }
        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="node">A syntax node related with the diagnostic.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic<TSyntax>(this SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, TSyntax node, params object[] messageArgs)
            where TSyntax : SyntaxNode
			=> context.ReportDiagnostic(Diagnostic.Create(descriptor, node.GetLocation(), messageArgs));

        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="nodes">Syntax nodes related with the diagnostic.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic<TSyntax>(this SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, IEnumerable<TSyntax> nodes, params object[] messageArgs)
            where TSyntax : SyntaxNode
			=> ReportDiagnostic(context, descriptor, nodes.Select(node => node.GetLocation()), messageArgs);

        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="locations">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic(this SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, IEnumerable<Location> locations, params object[] messageArgs)
        {
			var location = locations.FirstOrDefault() ?? Location.None;
            context.ReportDiagnostic(Diagnostic.Create(descriptor, location, locations.Skip(1), messageArgs));
        }

        /// <summary>Reports a <see cref="Diagnostic"/> related with a descriptor.</summary>
        /// <param name="context">Context to report a diagnostic.</param>
        /// <param name="descriptor">A <see cref="DiagnosticDescriptor"/> describing the diagnostic.</param>
        /// <param name="locations">An optional primary location of the diagnostic. If null, <see cref="Location"/> will return <see cref="Location.None"/>.</param>
        /// <param name="messageArgs">Arguments to the message of the diagnostic</param>
        public static void ReportDiagnostic(this SyntaxNodeAnalysisContext context, DiagnosticDescriptor descriptor, ImmutableArray<Location> locations, params object[] messageArgs)
        {
            context.ReportDiagnostic(Diagnostic.Create(descriptor, locations[0], locations.Skip(1), messageArgs));
        }    }
}