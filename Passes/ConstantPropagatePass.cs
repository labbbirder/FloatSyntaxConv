using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
                    .Select(n => n.Value)
                    ;
                var fillingArguments = ivkNode.ArgumentList.Arguments;
                bool hasFloatingDefault = false;
                var paramIndex = 0;
                foreach (var param in paramList.Parameters)
                {
                    paramIndex++;
                    var defaultParamValue = param.DescendantNodes()
                        .OfType<EqualsValueClauseSyntax>()
                        .Select(v => v.Value)
                        .FirstOrDefault()
                        ;
                    if (defaultParamValue is null) continue;

                    ExpressionSyntax defaultExpression = default!;
                    if (defaultParamValue is MemberAccessExpressionSyntax or IdentifierNameSyntax)
                    {
                        // get defining field
                        var declModel = compilation.GetSemanticModel(defaultParamValue.SyntaxTree);
                        var symb = declModel.GetSymbolInfo(defaultParamValue);
                        var field = symb.Symbol as IFieldSymbol;
                        if (field is null) continue;

                        // check floating type
                        hasFloatingDefault |= field.Type.SpecialType
                            is SpecialType.System_Single
                            or SpecialType.System_Double;
                        defaultExpression = GetFieldValue(field); // field.ConstValue returns null
                    }
                    else
                    {
                        var p = defaultParamValue.Ancestors().OfType<ParameterSyntax>().First();
                        hasFloatingDefault |= p.Type.ToString() is "float" or "double";
                        defaultExpression = defaultParamValue;
                    }
                    //else
                    //{
                    //    throw new($"Unknown syntax {defaultParamValue.GetType()}:{defaultParamValue.Parent}");
                    //}

                    var argument = SyntaxFactory.Argument(defaultExpression);
                    if (fillingArguments.Count < paramIndex)
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