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
    internal class ReplaceNumericLiteralPass : PassBase {
        double fs = 123.3f+ 1e3+123;
        Func<float,string> replacer;
        public ReplaceNumericLiteralPass(Func<float, string> replacer) {
            this.replacer = replacer;
        }
        internal override SyntaxNode Transform(SyntaxNode root, SemanticModel model) {
            var tokens = root.DescendantTokens()
                .OfType<SyntaxToken>()
                .Where(t => t.IsKind(SyntaxKind.NumericLiteralToken))
                .Where(t=>t.Value is float or double)
                ;
            var dict = new Dictionary<SyntaxNode, SyntaxNode>();
            foreach(var token in tokens) { 
                var repl = replacer(Convert.ToSingle(token.ValueText));
                var node = SyntaxFactory.ParseExpression(repl);
                dict.Add(token.Parent, node);
            }
            root = root.ReplaceNodes(tokens.Select(t=>t.Parent), (o, n) => {
                return dict[o];
            });
            return root;
        }
    }
}
