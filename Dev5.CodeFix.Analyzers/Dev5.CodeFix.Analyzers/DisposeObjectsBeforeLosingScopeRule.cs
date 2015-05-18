using System.Linq;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Dev5.CodeFix.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DisposeObjectsBeforeLosingScopeRule : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DisposeObjectsBeforeLosingScopeRule";
        internal const string Title = "Dispose objects before losing scope";
        internal const string MessageFormat = "A local object of a IDisposable type is created but the object is not disposed before all references to the object are out of scope.";
        internal const string Category = "Reliability";
        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(Rule); }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.VariableDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.SimpleAssignmentExpression);
        }

        /// <summary>
        /// Gets type symbol for System.IDisposable interface.
        /// </summary>
        /// <param name="compilation"></param>
        /// <returns></returns>
        public static INamedTypeSymbol IDisposable(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.IDisposable");
        }

        /// <summary>
        /// Returns boolean value indicating if the <paramref name="typeInfo"/> implements System.IDisposable interface.
        /// </summary>
        /// <param name="typeInfo">TypeInfo to check</param>
        /// <param name="compilation"></param>
        /// <returns></returns>
        private static bool IsDisposable(TypeInfo typeInfo, Compilation compilation)
        {
            if(typeInfo.Type == null)
            {
                return false;
            }
            return !typeInfo.Type.IsValueType && typeInfo.Type.AllInterfaces.Any(i => i.Equals(IDisposable(compilation)));
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var semanticModel = context.SemanticModel;
            var compilation = context.SemanticModel.Compilation;
            // are we inside using block? i.e. is the Parent of current node UsingStatement
            var insideUsingStatement = context.Node.Parent is UsingStatementSyntax;

            var declaration = context.Node as VariableDeclarationSyntax;
            // variable declaration node
            if (declaration != null)
            {
                // more than one variable can be declared
                foreach (var declarator in declaration.Variables)
                {
                    var variable = declarator.Identifier;
                    var variableSymbol = semanticModel.GetDeclaredSymbol(declarator);
                    var eq = declarator.Initializer as EqualsValueClauseSyntax;
                    var varTypeInfo = semanticModel.GetTypeInfo(eq?.Value);
                    // non-disposable variable is declared or currently inside using block
                    if (!IsDisposable(varTypeInfo, compilation) || insideUsingStatement)
                    {
                        continue;
                    }

                    // report this
                    context.ReportDiagnostic(Diagnostic.Create(Rule, declarator.GetLocation()));
                }
                return;
            }

            var assignment = context.Node as AssignmentExpressionSyntax;
            if (assignment != null)
            {
                // does the type of the RHS node implement IDisposable?
                var typeInfo = semanticModel.GetTypeInfo(assignment.Right);                
                if (!IsDisposable(typeInfo, compilation))
                {
                    return;
                }

                var identifier = assignment.Left as IdentifierNameSyntax;
                var kind = semanticModel.GetSymbolInfo(identifier).Symbol;
                // assigning field value or currently inside using block
                if (kind?.Kind == SymbolKind.Field || insideUsingStatement)
                {
                    return;
                }

                // report this
                context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.GetLocation()));
                return;
            }
        }
    }
}
