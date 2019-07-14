using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace FluentMigratorAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FluentMigratorAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        internal const string MissingDiagnosticId = "FM_Missing";
        private static readonly string MissingMigrationTitle = "Missing migration attribute";
        private static readonly string MissingMigrationMessageFormat = "The migration attribute is missing";
        private static readonly string MissingMigrationDescription = "Migration attribute is missing but can be generated";

        private const string FormatDiagnosticId = "FM_Invalid";
        private static readonly string FormatInvalidMigrationTitle = "The migration number has incorrect format";
        private static readonly string FormatInvalidMigrationMessageFormat = "The migration number {0} should be yyyyMMddhhmm";
        private static readonly string FormatInvalidMigrationDescription = "The migration number has to be of specific format";

        private const string Category = "Migrations";

        private static DiagnosticDescriptor MissingRule = new DiagnosticDescriptor
            (MissingDiagnosticId,
            MissingMigrationTitle,
            MissingMigrationMessageFormat,
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: MissingMigrationDescription);

        private static DiagnosticDescriptor FormatInvalidRule = new DiagnosticDescriptor
        (FormatDiagnosticId,
        FormatInvalidMigrationTitle,
        FormatInvalidMigrationMessageFormat,
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: FormatInvalidMigrationDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(MissingRule, FormatInvalidRule); } }

        public override void Initialize(AnalysisContext context)
        {   
            context.RegisterSyntaxNodeAction<SyntaxKind>(AnalyzeAttribute, new SyntaxKind[] { SyntaxKind.Attribute });
            context.RegisterSyntaxNodeAction<SyntaxKind>(AnalyzeType, new SyntaxKind[] { SyntaxKind.ClassDeclaration });
        }

        private static void AnalyzeType(SyntaxNodeAnalysisContext context)
        {
            ClassDeclarationSyntax node = context.Node as ClassDeclarationSyntax;

            if(node == null)
            {
                return;
            }

            var isValid = node.BaseList.Types.Any(t => t.ToString() == "Migration");

            if (!isValid)
            {
                return;
            }

            var attributeExists = node.AttributeLists.SelectMany(a => a.Attributes).Any(a => a.Name.ToString() == "Migration");
            if (!attributeExists)
            {
                context.ReportDiagnostic(Diagnostic.Create(MissingRule, node.GetLocation()));
            }
        }
        private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
        {
            DateTime dateTime;
            AttributeSyntax node = context.Node as AttributeSyntax;
            if (node == null)
            {
                return;
            }

            if (node.Name.ToString() != "Migration")
            {
                return;
            }

            if (node.ArgumentList == null)
            {
                return;
            }

            var versionArg = node.ArgumentList.Arguments.FirstOrDefault();
            if (versionArg != null && !DateTime.TryParseExact(versionArg.Expression.ToString(), "yyyyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                context.ReportDiagnostic(Diagnostic.Create(FormatInvalidRule, versionArg.GetLocation(), versionArg.Expression.ToString()));
            }
        }
    }
}
