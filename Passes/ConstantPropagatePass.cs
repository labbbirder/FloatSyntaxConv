using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloatSyntaxConv.Passes
{

    internal class ConstantPropagatePass : PassBase
    {

        public ConstantPropagatePass()
        {
        }

        static ExpressionSyntax GetFieldValue(IFieldSymbol symbol)
        {
            return symbol.DeclaringSyntaxReferences[0].GetSyntax()
                .DescendantNodes()
                .OfType<EqualsValueClauseSyntax>()
                .Single().Value;
        }

        internal override SyntaxNode Transform(SyntaxNode root, CSharpCompilation compilation)
        {
            var nodes = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                ;
            var thisModel = compilation.GetSemanticModel(root.SyntaxTree);
            var map = new Dictionary<SyntaxNode, SyntaxNode>();
            foreach (var ivkNode in nodes)
            {
                SymbolInfo sym = default;
                try
                {
                    sym = thisModel.GetSymbolInfo(ivkNode);
                }
                catch { continue; }
                var methodSymbol = sym.Symbol as IMethodSymbol;
                if (methodSymbol is null) continue;

                var decl = methodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
                var paramList = decl.ChildNodes().OfType<ParameterListSyntax>().First();

                // get default values
                var defaultParamValues = paramList.DescendantNodes()
                    .OfType<EqualsValueClauseSyntax>()
                    .Where(n => n.Value is MemberAccessExpressionSyntax or IdentifierNameSyntax) // ignore literal values
                    .Select(n => n.Value)
                    ;
                var fillingArguments = ivkNode.ArgumentList.Arguments;
                bool hasFloatingDefault = false;
                foreach (var v in defaultParamValues)
                {
                    // get defining field
                    var declModel = compilation.GetSemanticModel(v.SyntaxTree);
                    var symb = declModel.GetSymbolInfo(v);
                    var field = symb.Symbol as IFieldSymbol;
                    if (field is null) continue;

                    // check floating type
                    hasFloatingDefault |= field.Type.SpecialType
                        is SpecialType.System_Single
                        or SpecialType.System_Double;

                    var defaultValue = GetFieldValue(field); // field.ConstValue returns null
                    var argument = SyntaxFactory.Argument(GetFieldValue(field));
                    if (fillingArguments.Count < paramList.Parameters.Count)
                    {
                        fillingArguments = fillingArguments.Add(argument);
                    }
                }
                if (!hasFloatingDefault) continue;

                // fill invocation arguments
                var filledArgumentsSyntax = SyntaxFactory.ArgumentList(fillingArguments);
                map.Add(ivkNode, ivkNode.WithArgumentList(filledArgumentsSyntax));
            }
            root = root.ReplaceNodes(map.Keys, (o, n) => map[o]);
            return root;
        }
    }
}