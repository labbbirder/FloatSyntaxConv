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
        internal override SyntaxNode Transform(SyntaxNode root, CSharpCompilation compilation) {
            var nodes = root.DescendantNodes()
                .OfType<FieldDeclarationSyntax>()
                .Where(n=> types.Contains(n.Declaration.Type.ToString()))
                .SelectMany(n=>n.DescendantTokens().Where(t=>t.IsKind(SyntaxKind.ConstKeyword)))
                ;
            var dict = new Dictionary<SyntaxNode, SyntaxNode>();
            foreach(var n in nodes)
            {
                var ds = n.Parent as FieldDeclarationSyntax;
                if (ds is null) continue;
                var m = ds.Modifiers;
                var tokenStatic = SyntaxFactory.Token(SyntaxKind.StaticKeyword).WithTriviaFrom(n);
                var tokenReadonly = SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword).WithTrailingTrivia(n.TrailingTrivia);
                dict.Add(n.Parent!, ds.WithModifiers(m.Remove(n).Add(tokenStatic).Add(tokenReadonly)));
            }
            root = root.ReplaceNodes(dict.Keys,(o,n)=>dict[o])!;
            return root;
        }
    }
}
