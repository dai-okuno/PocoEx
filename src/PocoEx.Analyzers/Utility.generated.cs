using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;

namespace PocoEx
{
    partial class Utils
    {

        /// <summary>Gets the declaration syntaxes of <see cref="INamedTypeSymbol"/>.</summary>
        /// <param name="symbol">The symbol to get declarations.</param>
        public static Task<TypeDeclarationSyntax[]> GetDeclarationSyntaxAsync(this INamedTypeSymbol symbol)
            => GetDeclarationSyntaxAsync<TypeDeclarationSyntax>(symbol);

        /// <summary>Gets the declaration syntaxes of <see cref="IMethodSymbol"/>.</summary>
        /// <param name="symbol">The symbol to get declarations.</param>
        public static Task<MethodDeclarationSyntax[]> GetDeclarationSyntaxAsync(this IMethodSymbol symbol)
            => GetDeclarationSyntaxAsync<MethodDeclarationSyntax>(symbol);

        /// <summary>Gets the declaration syntaxes of <see cref="IParameterSymbol"/>.</summary>
        /// <param name="symbol">The symbol to get declarations.</param>
        public static Task<ParameterSyntax[]> GetDeclarationSyntaxAsync(this IParameterSymbol symbol)
            => GetDeclarationSyntaxAsync<ParameterSyntax>(symbol);

        /// <summary>Gets the declaration syntaxes of <see cref="IPropertySymbol"/>.</summary>
        /// <param name="symbol">The symbol to get declarations.</param>
        public static Task<PropertyDeclarationSyntax[]> GetDeclarationSyntaxAsync(this IPropertySymbol symbol)
            => GetDeclarationSyntaxAsync<PropertyDeclarationSyntax>(symbol);

        /// <summary>Gets the declaration syntaxes of <see cref="IEventSymbol"/>.</summary>
        /// <param name="symbol">The symbol to get declarations.</param>
        public static Task<EventDeclarationSyntax[]> GetDeclarationSyntaxAsync(this IEventSymbol symbol)
            => GetDeclarationSyntaxAsync<EventDeclarationSyntax>(symbol);

        /// <summary>Gets the location of <see cref="TypeDeclarationSyntax.Identifier"/>.</summary>
        /// <param name="nodes">Nodes to get the location of the identifier.</param>
        /// <returns>The locations of the identifier.</returns>
        public static Location[] GetIdentifierLocations(this TypeDeclarationSyntax[] nodes)
        {
            var result = new Location[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                result[i] = nodes[i].Identifier.GetLocation();
            }
            return result;
        }

        /// <summary>Gets the location of <see cref="MethodDeclarationSyntax.Identifier"/>.</summary>
        /// <param name="nodes">Nodes to get the location of the identifier.</param>
        /// <returns>The locations of the identifier.</returns>
        public static Location[] GetIdentifierLocations(this MethodDeclarationSyntax[] nodes)
        {
            var result = new Location[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                result[i] = nodes[i].Identifier.GetLocation();
            }
            return result;
        }

        /// <summary>Gets the location of <see cref="PropertyDeclarationSyntax.Identifier"/>.</summary>
        /// <param name="nodes">Nodes to get the location of the identifier.</param>
        /// <returns>The locations of the identifier.</returns>
        public static Location[] GetIdentifierLocations(this PropertyDeclarationSyntax[] nodes)
        {
            var result = new Location[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                result[i] = nodes[i].Identifier.GetLocation();
            }
            return result;
        }

        /// <summary>Gets the location of <see cref="EventDeclarationSyntax.Identifier"/>.</summary>
        /// <param name="nodes">Nodes to get the location of the identifier.</param>
        /// <returns>The locations of the identifier.</returns>
        public static Location[] GetIdentifierLocations(this EventDeclarationSyntax[] nodes)
        {
            var result = new Location[nodes.Length];
            for (int i = 0; i < nodes.Length; i++)
            {
                result[i] = nodes[i].Identifier.GetLocation();
            }
            return result;
        }
    }
}