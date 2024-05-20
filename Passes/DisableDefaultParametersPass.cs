using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FloatSyntaxConv.Passes {

    internal class DisableDefaultParametersPass : PassBase {

        public DisableDefaultParametersPass() {
            //this.option = option;
        }
        internal override SyntaxNode Transform(SyntaxNode root, CSharpCompilation compilation) {
            var nodes = root.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                ;
            foreach(var node in nodes) {
                SymbolInfo sym = default;
                try {
                    //sym = model.GetSymbolInfo(node);

                }
                catch {

                }
                //Console.WriteLine(node+": "+sym.Symbol);
            }
            return root;
        }
    }
}
