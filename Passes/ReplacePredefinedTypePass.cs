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
    internal class ReplacePredefinedTypePass : PassBase {
        string origin, replace;
        public ReplacePredefinedTypePass(string origin, string replace) {
            this.origin = origin;
            this.replace = replace;
        }
        internal override SyntaxNode Transform(SyntaxNode root, CSharpCompilation compilation) {
            var nodes = root.DescendantNodes()
                .OfType<PredefinedTypeSyntax>()
                .Where(n => n.ToString() == origin)
                ;
            root = root.ReplaceNodes(nodes, (o, n) => {
                var result = n.ToFullString().Replace(origin, replace);
                return SyntaxFactory.IdentifierName(result);
            });
            return root;
        }
    }
}
