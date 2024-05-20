using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FloatSyntaxConv.Passes
{

    internal class DisableDefaultParametersPass : PassBase
    {

        public DisableDefaultParametersPass()
        {
            //this.option = option;
        }
        internal override SyntaxNode Transform(SyntaxNode root, CSharpCompilation compilation)
        {
            var paramLists = root.DescendantNodes()
                .OfType<ParameterListSyntax>()
                ;

            var list = new List<SyntaxNode>();
            foreach (var paramList in paramLists)
            {
                // get default values
                var defaultParamValues = paramList.DescendantNodes()
                    .OfType<EqualsValueClauseSyntax>()
                    .Select(n => n.Value)
                    ;
                bool hasFloatingDefault = false;
                var defaultSyntaxs = new List<SyntaxNode>();
                foreach (var v in defaultParamValues)
                {
                    var p = v.Ancestors().OfType<ParameterSyntax>().First();
                    // check floating type
                    hasFloatingDefault |= p.Type is PredefinedTypeSyntax pdt
                        && pdt.ToString() is "float" or "double";
                    defaultSyntaxs.Add(v.Parent!);
                }
                if (!hasFloatingDefault) continue;
                list.AddRange(defaultSyntaxs);
            }
            root = root.RemoveNodes(list, SyntaxRemoveOptions.KeepNoTrivia)!;
            return root;
        }
    }
}
