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
    internal class DisableFieldConstPass : PassBase {
        HashSet<string> types;
        public DisableFieldConstPass(HashSet<string> types) {
            this.types = types;
        }
        internal override SyntaxNode Transform(SyntaxNode root, SemanticModel model) {
            var nodes = root.DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .Where(n=> types.Contains(n.Declaration.Type.ToString()))
                .SelectMany(n=>n.DescendantTokens().Where(t=>t.IsKind(SyntaxKind.ConstKeyword)))
                ;
            var dict = new Dictionary<SyntaxNode, SyntaxNode>();
            foreach(var n in nodes)
            {
                Console.WriteLine(n.SyntaxTree.FilePath);
                Console.WriteLine(n.Parent);
                var ds = n.Parent as FieldDeclarationSyntax;
                if (ds is null) continue;
                var m = ds.Modifiers;

                dict.Add(n.Parent!, ds.WithModifiers(m.Remove(n).Add(SyntaxFactory.ParseToken("static "))));
            }
            root = root.ReplaceNodes(dict.Keys,(o,n)=>dict[o])!;
            return root;
        }
    }
}
