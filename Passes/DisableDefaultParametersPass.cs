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

    internal class DisableDefaultParametersPass : PassBase {

        public DisableDefaultParametersPass() {
            //this.option = option;
        }
        internal override SyntaxNode Transform(SyntaxNode root, SemanticModel model) {
            var nodes = root.DescendantNodes()
                .OfType<BlockSyntax>()
                .SelectMany(n=>n.DescendantNodesAndTokens().OfType<ConstantPatternSyntax>())
                ;
            InvocationExpressionSyntax s;
            root = root.RemoveNodes(nodes,SyntaxRemoveOptions.KeepNoTrivia)!;
            return root;
        }
    }
}
