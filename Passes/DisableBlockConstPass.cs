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
    internal enum DisableBlockOption{
        ReplaceVariablesWithConstant,
        RemoveConstKeywordOnly,
    }
    internal class DisableBlockConstPass : PassBase {
        DisableBlockOption option;
        public DisableBlockConstPass(DisableBlockOption option) {
            this.option = option;
        }
        internal override SyntaxNode Transform(SyntaxNode root, SemanticModel model) {
            var nodes = root.DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .SelectMany(n=>n.DescendantTokens().Where(t=>t.IsKind(SyntaxKind.ConstKeyword)))
                ;
            var dict = new Dictionary<SyntaxNode, SyntaxNode>();
            foreach(var n in nodes)
            {
                var ds = n.Parent as LocalDeclarationStatementSyntax;
                var m = ds!.Modifiers;

                dict.Add(n.Parent!, ds.WithModifiers(m.Remove(n)));
            }
            root = root.ReplaceNodes(dict.Keys,(o,n)=>dict[o])!;
            return root;
        }
    }
}
