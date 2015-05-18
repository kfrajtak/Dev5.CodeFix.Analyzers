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
        public static INamedTypeSymbol IDisposable(Compilation compilation)
        {
            return compilation.GetTypeByMetadataName("System.IDisposable");
        }
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
            var insideUsingStatement = context.Node.Parent is UsingStatementSyntax;

            var declaration = context.Node as VariableDeclarationSyntax;
            if (declaration != null)
            {
                foreach (var declarator in declaration.Variables)
                {
                    var variable = declarator.Identifier;
                    var variableSymbol = semanticModel.GetDeclaredSymbol(declarator);
                    var eq = declarator.Initializer as EqualsValueClauseSyntax;
                    var varTypeInfo = semanticModel.GetTypeInfo(eq?.Value);
                    if (!IsDisposable(varTypeInfo, compilation) 
                        || (IsDisposable(varTypeInfo, compilation) && insideUsingStatement))
                    {
                        continue;
                    }

                    var kind = semanticModel.GetDeclaredSymbol(declarator).Kind;
                    context.ReportDiagnostic(Diagnostic.Create(Rule, declarator.GetLocation()));
                }
                return;
            }

            var assignment = context.Node as AssignmentExpressionSyntax;
            if (assignment != null)
            {
                var rightTypeInfo = semanticModel.GetTypeInfo(assignment.Right);                

                if (!IsDisposable(rightTypeInfo, compilation))
                {
                    return;
                }

                var identifier = assignment.Left as IdentifierNameSyntax;
                var kind = semanticModel.GetSymbolInfo(identifier).Symbol;
                if(kind?.Kind == SymbolKind.Field || insideUsingStatement)
                {
                    return;
                }
                
                context.ReportDiagnostic(Diagnostic.Create(Rule, assignment.GetLocation()));
                return;
            }
        }
    }
}
