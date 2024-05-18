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
    [Obsolete]
    internal class ChangeUsingDeclPass : PassBase {
        string origin, replace;
        public ChangeUsingDeclPass(string origin,string replace) {
            this.origin = origin;
            this.replace = replace;
        }
        internal override SyntaxNode Transform(SyntaxNode root, SemanticModel model) {
            var nodes = root.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .Select(s=>s.ChildNodes().OfType<NameSyntax>().FirstOrDefault());
            foreach(var n in nodes) {
                if(n==null)continue;
                if (Regex.IsMatch(n.ToString(), origin)) {
                    var s = Regex.Replace(n.ToString(),origin, replace);
                    root = root.ReplaceNode(n, SyntaxFactory.IdentifierName(s));
                }
            }
            return root;
        }
    }
}
