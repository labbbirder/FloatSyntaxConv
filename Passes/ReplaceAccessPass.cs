using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloatSyntaxConv.Passes {
    internal class ReplaceAccessPass : PassBase {
        Func<string, string> replacer;
        public ReplaceAccessPass(Func<string,string> replacer) {
            this.replacer = replacer;
        }
        internal override SyntaxNode Transform(SyntaxNode root, CSharpCompilation compilation) {
            var nodes = root.DescendantNodes()
                .OfType<MemberAccessExpressionSyntax>()
                .Where(n => n.Parent is not MemberAccessExpressionSyntax)
                .Where(n=>n.ToString()!=replacer(n.ToString()))
                ;
            root = root.ReplaceNodes(nodes, (o, n) => {
                var repl = replacer(o.ToString());
                return SyntaxFactory.IdentifierName(repl);
            });
            return root;
        }
    }
}
