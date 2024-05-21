using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloatSyntaxConv.Passes {
    internal class ReplaceSizeOfPass : PassBase {
        Dictionary<string, string> replacer;
        public ReplaceSizeOfPass(Dictionary<string,string> replacer) {
            this.replacer = replacer;
        }
        internal override SyntaxNode Transform(SyntaxNode root, CSharpCompilation compilation) {
            var nodes = root.DescendantNodes()
                .OfType<SizeOfExpressionSyntax>()
                .Where(n=>replacer.ContainsKey(n.Type.ToString()))
                ;

            root = root.ReplaceNodes(nodes.Select(n=>n.Type), (o, n) => {
                var replType = replacer[o.ToString()];
                return SyntaxFactory.ParseExpression(replType);
            });
            return root;
        }
    }
}
