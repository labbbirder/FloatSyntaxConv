﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
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

        static ExpressionSyntax? GetFieldValue(IFieldSymbol symbol)
        {
            return symbol.DeclaringSyntaxReferences[0].GetSyntax()
                .DescendantNodes()
                .OfType<EqualsValueClauseSyntax>()
                .SingleOrDefault()?.Value;
        }

        internal override SyntaxNode Transform(SyntaxNode root, CSharpCompilation compilation)
        {
            var nodes = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                ;
            var thisModel = compilation.GetSemanticModel(root.SyntaxTree,true);
            var map = new Dictionary<SyntaxNode, SyntaxNode>();
            foreach (var ivkNode in nodes)
            {
                var fillingArguments = ivkNode.ArgumentList.Arguments;
                var explicitArgumentsCount = 0;
                SymbolInfo sym = default;
                try
                {
                    sym = thisModel.GetSymbolInfo(ivkNode);
                }
                catch { continue; }
                try
                {
                    var operation = thisModel.GetOperation(ivkNode) as IInvocationOperation;
                    explicitArgumentsCount = operation.Arguments.Count(a => a.ArgumentKind != ArgumentKind.DefaultValue);
                }
                catch
                {
                    explicitArgumentsCount = fillingArguments.Count;
                }
                var methodSymbol = sym.Symbol as IMethodSymbol;
                if (methodSymbol is null && sym.CandidateSymbols.Length > 0)
                {
                    var symbols = sym.CandidateSymbols.OfType<IMethodSymbol>()
                        .Where(s => s.Parameters.Length >= fillingArguments.Count)
                        .ToArray();

                    if (symbols.Length > 1) Console.WriteLine($"{ivkNode} with candidates count:{sym.CandidateSymbols.Length}");
                    if (symbols.Length != 0)
                    {
                        methodSymbol = symbols[0];
                    }
                }

                if (methodSymbol is null) continue;
                if (methodSymbol.DeclaringSyntaxReferences.Length == 0) continue;
                var decl = methodSymbol.DeclaringSyntaxReferences[0].GetSyntax();
                var paramList = decl.ChildNodes().OfType<ParameterListSyntax>().First();

                // get default values
                var defaultParamValues = paramList.DescendantNodes()
                    .OfType<EqualsValueClauseSyntax>()
                    .Select(n => n.Value)
                    ;
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

                    ExpressionSyntax? defaultExpression = default;
                    if (defaultParamValue is MemberAccessExpressionSyntax or IdentifierNameSyntax)
                    {
                        // get defining field
                        var declModel = compilation.GetSemanticModel(defaultParamValue.SyntaxTree, true);
                        var symb = declModel.GetSymbolInfo(defaultParamValue);
                        var field = symb.Symbol as IFieldSymbol;
                        if (field is null) continue;

                        // check floating type
                        hasFloatingDefault |= field.Type.SpecialType
                            is SpecialType.System_Single
                            or SpecialType.System_Double;
                        defaultExpression = GetFieldValue(field)  // field.ConstValue returns null
                            ?? defaultParamValue; // for default enums

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

                    if (explicitArgumentsCount < paramIndex)
                    {
                        explicitArgumentsCount++;
                        var argument = SyntaxFactory.Argument(defaultExpression);
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