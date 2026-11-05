using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Graphs.NET.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class DuplicateGenericNothingAnalyzer : DiagnosticAnalyzer
	{
		private static readonly DiagnosticDescriptor GraphRule = new(
			id: "GN0002",
			title: "Invalid Graph<TEdgeWeight, TVertexData> generic type parameters",
			messageFormat: "Using Graph<Nothing, Nothing> should be avoided. Use the non-generic 'Graph' class instead.",
			category: "Usage",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Recommends using non-generic Graph when no weights and vertex data are needed.");

		private static readonly DiagnosticDescriptor GraphBuilderRule = new(
			id: "GN0001",
			title: "Invalid GraphBuilder<TEdgeWeight, TVertexData> generic type parameters",
			messageFormat: "Using GraphBuilder<Nothing, Nothing> should be avoided. Use the non-generic 'GraphBuilder' class instead.",
			category: "Usage",
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Recommends using non-generic GraphBuilder when no weights and vertex data are needed.");

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [GraphRule, GraphBuilderRule];

		public override void Initialize (AnalysisContext context)
		{
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
			context.EnableConcurrentExecution();
			context.RegisterSyntaxNodeAction(AnalyzedGenericName, SyntaxKind.GenericName);
		}

		private static void AnalyzedGenericName (SyntaxNodeAnalysisContext context)
		{
			GenericNameSyntax genericType = (GenericNameSyntax) context.Node;

			string typeName = genericType.Identifier.Text;

			if ( (typeName != "Graph" && typeName != "GraphBuilder") || genericType.TypeArgumentList.Arguments.Count != 2 )
			{
				return;
			}
			TypeInfo type1 = context.SemanticModel.GetTypeInfo(genericType.TypeArgumentList.Arguments[0]);
			TypeInfo type2 = context.SemanticModel.GetTypeInfo(genericType.TypeArgumentList.Arguments[1]);

			if ( type1.Type.Name == "Nothing" && type2.Type.Name == "Nothing" )
			{
				DiagnosticDescriptor rule = typeName == "Graph"
					? GraphRule : GraphBuilderRule;
				Diagnostic diagnostic = Diagnostic.Create(rule, genericType.TypeArgumentList.GetLocation());
				context.ReportDiagnostic(diagnostic);
			}
		}
	}
}
