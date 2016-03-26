using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using PocoEx.Collections;
using PocoEx.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace PocoEx.CodeAnalysis.Equality
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public partial class EqualityAnalyzer
        : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PocoEx.Equality";

        private static readonly ImmutableArray<SymbolKind> NamedTypeTargets
            = ImmutableArray.Create(SymbolKind.NamedType);

        private static readonly Action<CodeBlockAnalysisContext> _AnalyzeCodeBlock = AnalyzeCodeBlock;

        private static readonly Action<SymbolAnalysisContext> _AnalyzeNamedType = AnalyzeNamedType;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
            = ImmutableArray.Create(
                Rules.PocoEx00101,
                Rules.PocoEx00102,
                Rules.PocoEx00103,
                Rules.PocoEx00104,
                Rules.PocoEx00105,
                Rules.PocoEx00106);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(_AnalyzeNamedType, NamedTypeTargets);
            context.RegisterCodeBlockAction(AnalyzeCodeBlock);
        }

        private static void AnalyzeCodeBlock(CodeBlockAnalysisContext context)
        {
            var method = context.OwningSymbol as IMethodSymbol;
            if (method == null
                || method.DeclaredAccessibility != Accessibility.Public
                || method.IsStatic
                || method.ReturnType.SpecialType != SpecialType.System_Boolean
                || method.Parameters.Length != 1
                || method.Name != "Equals") return;

            switch (method.Parameters[0].Type.SpecialType)
            {
                case SpecialType.None:
                    if (method.Parameters[0].Type.IsValueType)
                    {
                        var analyzer = new EqualsValueTypeAnalyzer();
                        analyzer.Analyze(context);
                    }
                    else
                    {
                        var analyzer = new EqualsReferenceTypeAnalyzer();
                        analyzer.Analyze(context);
                    }
                    break;
                case SpecialType.System_Object:
                    if (method.IsOverride)
                    {   // public override bool Equals(object obj)
                        var analyzer = new EqualsObjectAnalyzer();
                        analyzer.Analyze(context);
                    }
                    break;
                default:
                    break;
            }
        }

        private static void AnalyzeNamedType(SymbolAnalysisContext context)
        {
            var namedType = (INamedTypeSymbol)context.Symbol;
            var iEquatable = context.Compilation.GetTypeByMetadataName(typeof(IEquatable<>).FullName);
            AnalyzePocoEx00101(context, namedType, iEquatable);
            if (namedType.IsValueType)
            {
                var iEquatableSelf = iEquatable.Construct(namedType);
                AnalyzePocoEx00102(context, namedType, iEquatableSelf);
            }
        }

        private static void AnalyzePocoEx00101(SymbolAnalysisContext context, INamedTypeSymbol namedType, INamedTypeSymbol iEquatable)
        {
            foreach (var @interface in namedType.AllInterfaces)
            {
                if (!iEquatable.Equals(@interface.ConstructedFrom)) continue;
                // implements IEquatable<T>
                var methods = namedType.GetMembers(nameof(Equals));
                foreach (IMethodSymbol method in methods)
                {
                    if (method.DeclaredAccessibility == Accessibility.Public
                        && method.IsOverride
                        && method.Parameters.Length == 1
                        && method.Parameters[0].Type.SpecialType == SpecialType.System_Object)
                    {   // public override bool Equals(object)
                        return;
                    }
                }
                context.ReportDiagnostic(
                    Rules.PocoEx00101.ToDiagnostic(namedType.Locations));
                return;
            }
        }

        private static void AnalyzePocoEx00102(SymbolAnalysisContext context, INamedTypeSymbol namedType, INamedTypeSymbol iEquatableSelf)
        {
            foreach (var @interface in namedType.Interfaces)
            {
                if (iEquatableSelf.Equals(@interface))
                {   // implements IEquatable<namedType>
                    return;
                }
            }
            // IEquatable<namedType> is not implemented.
            context.ReportDiagnostic(
                Rules.PocoEx00102.ToDiagnostic(namedType.Locations,
                namedType.ToString()));
        }

        class EqualsObjectAnalyzer
            : CSharpSyntaxWalker
        {
            SemanticModel SemanticModel;

            IEnumerable<IMethodSymbol> Uninvoked;

            public void Analyze(CodeBlockAnalysisContext context)
            {

                SemanticModel = context.SemanticModel;
                var namedType = context.OwningSymbol.ContainingType;
                Uninvoked =
                    namedType.GetMembers(nameof(Equals))
                    .OfType<IMethodSymbol>()
                    .Where(m => m.DeclaredAccessibility == Accessibility.Public
                        && !m.IsStatic
                        && m.Parameters.Length == 1
                        && m.Parameters[0].Type.SpecialType != SpecialType.System_Object)
                    ;

                Visit(context.CodeBlock);
                var uninvoked = Format(Uninvoked);
                if (uninvoked != string.Empty)
                {
                    context.ReportDiagnostic(
                        Rules.PocoEx00103.ToDiagnostic(context.OwningSymbol.Locations,
                        uninvoked));
                }
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                var method = SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
                if (method != null
                    && method.ReturnType.SpecialType == SpecialType.System_Boolean
                    && method.Parameters.Length == 1
                    && method.Name == nameof(Equals))
                {   // bool Equals(T)
                    Uninvoked = Uninvoked.Remove(method);
                    return;
                }
                base.VisitInvocationExpression(node);
            }

            private string Format(IEnumerable<IMethodSymbol> methods)
            {
                var builder = new StringBuilder();
                using (var m = methods.GetEnumerator())
                {
                    if (!m.MoveNext()) return string.Empty;
                    builder.Append('\'').AppendSignature(m.Current).Append('\'');
                    while (m.MoveNext())
                    {
                        builder.Append(", ").Append('\'').AppendSignature(m.Current).Append('\'');
                    }
                }
                return builder.ToString();
            }

        }

        abstract class EqualsTAnalyzer
            : CSharpSyntaxWalker
        {

            private static readonly Func<ISymbol, bool> IsEqualityElement = (member) =>
            {
                if (member.IsStatic
                    || member.DeclaredAccessibility != Accessibility.Public) return false;

                switch (member.Kind)
                {
                    case SymbolKind.Field:
                        var field = (IFieldSymbol)member;
                        return !field.IsConst && !field.IsImplicitlyDeclared;
                    case SymbolKind.Property:
                        var property = (IPropertySymbol)member;
                        return property.GetMethod != null
                            && property.GetMethod.DeclaredAccessibility == Accessibility.Public;
                    default:
                        return false;
                }
            };
            protected CodeBlockAnalysisContext Context { get; private set; }

            protected ImmutableArray<string> SuppressedMemberNames { get; private set; }

            protected INamedTypeSymbol IgnoreAttributeSymbol { get; private set; }

            protected IMethodSymbol Target { get; private set; }

            private IEnumerable<ISymbol> Untouched;

            public void Analyze(CodeBlockAnalysisContext context)
            {
                Analyzing(context);
                Visit(context.CodeBlock);
                Analyzed(context);
            }

            public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
            {
                var symbol = Context.SemanticModel.GetSymbolInfo(node);
                if (symbol.CandidateReason == CandidateReason.None
                    && !symbol.Symbol.IsStatic
                    && symbol.Symbol.DeclaredAccessibility == Accessibility.Public
                    && Target.Parameters[0].Type.Equals(symbol.Symbol.ContainingType))
                {
                    switch (symbol.Symbol.Kind)
                    {
                        case SymbolKind.Property:
                            var property = (IPropertySymbol)symbol.Symbol;
                            if (property.GetMethod != null && property.GetMethod.DeclaredAccessibility == Accessibility.Public)
                            {   // getter is public
                                Untouched = Untouched.Remove(property);
                            }
                            break;
                        case SymbolKind.Field:
                            var field = (IFieldSymbol)symbol.Symbol;
                            if (!field.IsConst)
                            {
                                Untouched = Untouched.Remove(field);
                            }
                            break;
                    }
                }
                base.VisitMemberAccessExpression(node);
            }

            protected virtual void Analyzed(CodeBlockAnalysisContext context)
            {
                foreach (var untouched in Untouched)
                {
                    if (SuppressedMemberNames.Contains(untouched.Name)) continue;
                    context.ReportDiagnostic(
                        Rules.PocoEx00106.ToDiagnostic(Target.Locations,
                        Target.Parameters[0].Type,
                        untouched.Name));
                }
            }

            protected virtual void Analyzing(CodeBlockAnalysisContext context)
            {
                Target = (IMethodSymbol)context.OwningSymbol;
                Context = context;
                IgnoreAttributeSymbol = Context.SemanticModel.Compilation.GetTypeByMetadataName("PocoEx.CodeAnalysis.EqualityIgnoreAttribute");
                var ignore = Target.GetAttributes().FirstOrDefault(attr => IgnoreAttributeSymbol.Equals(attr.AttributeClass));
                if (ignore == null
                    || ignore.ConstructorArguments.Length < 1
                    || ignore.ConstructorArguments[0].Values.Length < 1)
                {
                    SuppressedMemberNames = ImmutableArray<string>.Empty;
                }
                else
                {
                    var builder = ImmutableArray.CreateBuilder<string>(ignore.ConstructorArguments[0].Values.Length);
                    foreach (var constant in ignore.ConstructorArguments[0].Values)
                    {
                        if (constant.Type.SpecialType != SpecialType.System_String) continue;
                        var name = (string)constant.Value;
                        if (string.IsNullOrEmpty(name)) continue;
                        builder.Add(name);
                    }
                    SuppressedMemberNames = builder.MoveToImmutable();
                }
                var parameterType = Target.Parameters[0].Type;
                if (IsSupportedType(parameterType))
                {
                    Untouched = parameterType.GetMembers()
                        .Where(
                            Target.ContainingType.Equals(parameterType) ? IsEqualityElementOfSelf
                            : Target.ContainingType.IsImplements(parameterType) ? IsEqualityElementOfImplemented
                            : Target.ContainingType.IsSubTypeOf(parameterType) ? IsEqualityElementOfAncestor
                            : IsEqualityElement);
                }
                else
                {
                    Untouched = Empty.Enumerable<ISymbol>();
                }
            }

            private bool IsEqualityElementOfAncestor(ISymbol member)
            {
                if (member.IsStatic) return false;
                switch (member.Kind)
                {
                    case SymbolKind.Field:
                        var field = (IFieldSymbol)member;
                        if (field.IsConst || field.IsImplicitlyDeclared)
                        {   // const or auto-backing-field
                            return false;
                        }
                        else
                        {   // declared
                            switch (field.DeclaredAccessibility)
                            {
                                case Accessibility.Public:
                                case Accessibility.Protected:
                                case Accessibility.ProtectedOrInternal:
                                    return true;
                                case Accessibility.NotApplicable:
                                case Accessibility.Private:
                                    return false;
                                case Accessibility.ProtectedAndInternal:
                                case Accessibility.Internal:
                                    return IsInternal(field);
                                default:
                                    throw Utils.UnexpectedEnum(field.DeclaredAccessibility);
                            }
                        }
                    case SymbolKind.Property:
                        var property = (IPropertySymbol)member;
                        if (property.GetMethod == null || !property.GetMethod.IsImplicitlyDeclared)
                        {   // write-only or not auto-property
                            return false;
                        }
                        else
                        {   // declared
                            switch (property.GetMethod.DeclaredAccessibility)
                            {
                                case Accessibility.Public:
                                case Accessibility.Protected:
                                case Accessibility.ProtectedOrInternal:
                                    return true;
                                case Accessibility.NotApplicable:
                                case Accessibility.Private:
                                    return false;
                                case Accessibility.ProtectedAndInternal:
                                case Accessibility.Internal:
                                    return IsInternal(property);
                                default:
                                    throw Utils.UnexpectedEnum(property.DeclaredAccessibility);
                            }
                        }
                    default:
                        return false;
                }
            }

            private bool IsEqualityElementOfImplemented(ISymbol member)
                => member.Kind == SymbolKind.Property;

            private bool IsEqualityElementOfSelf(ISymbol member)
            {
                if (member.IsStatic) return false;
                switch (member.Kind)
                {
                    case SymbolKind.Field:
                        var field = (IFieldSymbol)member;
                        if (field.IsConst || field.IsImplicitlyDeclared)
                        {   // const or auto-backing-field
                            return false;
                        }
                        else if (field.IsDefinition)
                        {   // declared
                            return true;
                        }
                        else
                        {   // derived
                            switch (field.DeclaredAccessibility)
                            {
                                case Accessibility.Public:
                                case Accessibility.Protected:
                                case Accessibility.ProtectedOrInternal:
                                    return true;
                                case Accessibility.NotApplicable:
                                case Accessibility.Private:
                                    return false;
                                case Accessibility.ProtectedAndInternal:
                                case Accessibility.Internal:
                                    return IsInternal(field);
                                default:
                                    throw Utils.UnexpectedEnum(field.DeclaredAccessibility);
                            }
                        }
                    case SymbolKind.Property:
                        var property = (IPropertySymbol)member;
                        if (property.IsWriteOnly) return false;
                        // has getter
                        var getter = property.GetMethod.DeclaringSyntaxReferences[0].GetSyntax(Context.CancellationToken) as AccessorDeclarationSyntax;
                        if (getter == null || getter.Body != null) return false;
                        // auto-property
                        if (property.IsDefinition) return true;
                        // derived
                        switch (property.GetMethod.DeclaredAccessibility)
                        {
                            case Accessibility.Public:
                            case Accessibility.Protected:
                            case Accessibility.ProtectedOrInternal:
                                return true;
                            case Accessibility.NotApplicable:
                            case Accessibility.Private:
                                return false;
                            case Accessibility.ProtectedAndInternal:
                            case Accessibility.Internal:
                                return IsInternal(property);
                            default:
                                throw Utils.UnexpectedEnum(property.DeclaredAccessibility);
                        }
                    default:
                        return false;
                }
            }

            private bool IsInternal(ISymbol symbol)
                => Context.SemanticModel.Compilation.Assembly.Equals(symbol.ContainingAssembly);

            private bool IsSupportedType(ITypeSymbol parameterType)
            {
                switch (parameterType.SpecialType)
                {
                    case SpecialType.System_String:
                    case SpecialType.System_Boolean:
                    case SpecialType.System_Byte:
                    case SpecialType.System_Int16:
                    case SpecialType.System_Int32:
                    case SpecialType.System_Int64:
                    case SpecialType.System_SByte:
                    case SpecialType.System_UInt16:
                    case SpecialType.System_UInt32:
                    case SpecialType.System_UInt64:
                    case SpecialType.System_Decimal:
                    case SpecialType.System_Double:
                    case SpecialType.System_Single:
                    case SpecialType.System_Char:
                    case SpecialType.System_IntPtr:
                    case SpecialType.System_UIntPtr:
                        return false;
                    default:
                        switch (parameterType.TypeKind)
                        {
                            case TypeKind.Class:
                            case TypeKind.Interface:
                            case TypeKind.Struct:
                            case TypeKind.Unknown:
                                return true;
                            default:
                                return false;
                        }
                }
            }

        }

        class EqualsValueTypeAnalyzer
            : EqualsTAnalyzer
        { }

        class EqualsReferenceTypeAnalyzer
            : EqualsTAnalyzer
        {

            private IMethodSymbol Object_ReferenceEquals;

            private bool ParameterNullCheckFound;

            private bool ParameterThisCheckFound;

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (!ParameterThisCheckFound || !ParameterNullCheckFound)
                {
                    var symbol = Context.SemanticModel.GetSymbolInfo(node);
                    if (symbol.CandidateReason == CandidateReason.None
                        && Object_ReferenceEquals.Equals(symbol.Symbol))
                    {
                        if (!ParameterNullCheckFound && IsParameterNullCheck(node))
                        {
                            ParameterNullCheckFound = true;
                        }
                        if (!ParameterThisCheckFound && IsParameterThisCheck(node))
                        {
                            ParameterThisCheckFound = true;
                        }
                    }
                }
                base.VisitInvocationExpression(node);
            }
            protected override void Analyzed(CodeBlockAnalysisContext context)
            {
                base.Analyzed(context);
                if (!ParameterNullCheckFound)
                {
                    context.ReportDiagnostic(
                        Rules.PocoEx00104.ToDiagnostic(Target.Locations,
                        Target.Parameters[0].Name));
                }
                if (!ParameterThisCheckFound)
                {
                    context.ReportDiagnostic(
                        Rules.PocoEx00105.ToDiagnostic(Target.Locations,
                        Target.Parameters[0].Name));
                }
            }
            protected override void Analyzing(CodeBlockAnalysisContext context)
            {
                base.Analyzing(context);

                Object_ReferenceEquals = (IMethodSymbol)Context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(object).FullName)
                    .GetMembers(nameof(ReferenceEquals))
                    .First();
            }

            private bool IsParameter(SyntaxNode node)
            {
                var symbol = Context.SemanticModel.GetSymbolInfo(node);
                return symbol.CandidateReason == CandidateReason.None
                    && Target.Parameters[0].Equals(symbol.Symbol);
            }

            private bool IsParameterNullCheck(InvocationExpressionSyntax node)
            {
                var x = node.ArgumentList.Arguments[0].Expression;
                var y = node.ArgumentList.Arguments[1].Expression;
                if (x.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    return IsParameter(y);
                }
                else if (y.IsKind(SyntaxKind.NullLiteralExpression))
                {
                    return IsParameter(x);
                }
                else
                {
                    return false;
                }
            }

            private bool IsParameterThisCheck(InvocationExpressionSyntax node)
            {
                var x = node.ArgumentList.Arguments[0].Expression;
                var y = node.ArgumentList.Arguments[1].Expression;
                if (x.IsKind(SyntaxKind.ThisExpression))
                {
                    return IsParameter(y);
                }
                else if (y.IsKind(SyntaxKind.ThisExpression))
                {
                    return IsParameter(x);
                }
                else
                {
                    return false;
                }
            }

        }
    }
}
