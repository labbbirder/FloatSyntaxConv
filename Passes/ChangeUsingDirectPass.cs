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
    internal class ChangeUsingDirectPass : PassBase {
        Func<string, string> replacer;
        public ChangeUsingDirectPass(Func<string,string> replacer) {
            this.replacer = replacer;
        }
        internal override SyntaxNode Transform(SyntaxNode root, SemanticModel model) {
            var nodes = root.DescendantNodes()
                .Where(n=>n is UsingDirectiveSyntax or NamespaceDeclarationSyntax or TypeOfExpressionSyntax)
                .SelectMany(s=>s.ChildNodes().OfType<NameSyntax>());
            root = root.ReplaceNodes(nodes, (o, n) => {
                var repl = replacer(o.ToString());
                return SyntaxFactory.IdentifierName(repl);
            });
            return root;
        }
    }
}
