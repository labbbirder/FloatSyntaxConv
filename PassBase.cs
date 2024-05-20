using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FloatSyntaxConv {

    internal enum VisitPhase{
        Unknown,
        Collecting,
        Transforming,
        Final,
    }
    internal abstract class PassBase {
        internal abstract SyntaxNode Transform(SyntaxNode root, CSharpCompilation compilation);
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
