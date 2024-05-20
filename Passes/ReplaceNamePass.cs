using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FloatSyntaxConv.Passes {
    internal class ReplaceNamePass : PassBase {
        Func<string, string> replacer;
        public ReplaceNamePass(Func<string,string> replacer) {
            this.replacer = replacer;
        }
        internal override SyntaxNode Transform(SyntaxNode root, CSharpCompilation compilation) {
            var nodes = root.DescendantNodes()
                .OfType<NameSyntax>()
                .Where(n => n.Parent is not NameSyntax || n.Parent is TypeArgumentListSyntax)
                .Where(n=>n.ToString()!=replacer(n.ToString()))
                ;
            root = root.ReplaceNodes(nodes, (o, n) => {
                var repl = replacer(o.ToString());
                if (o.Parent is TypeArgumentListSyntax) Console.WriteLine(o.Parent +" > "+repl );
                return SyntaxFactory.IdentifierName(repl);
            });
            return root;
        }
    }
}
