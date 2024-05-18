using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FloatSyntaxConv {
    internal abstract class PassBase {
        internal abstract SyntaxNode Transform(SyntaxNode root);
        protected bool IsWildcardMatch(string Wildcard, string input) {
            Wildcard = Wildcard
                .Replace(".", "\\.")
                .Replace("?", ".")
                .Replace("*", ".*")
                ;
            return Regex.IsMatch(input, Wildcard) ;
        }
    }
}
