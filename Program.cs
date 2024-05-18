// See https://aka.ms/new-console-template for more information
using FloatSyntaxConv;
using FloatSyntaxConv.Passes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

Console.WriteLine("Hello, World!");


//void VisitDirectory(string directory,string output) {
//    var csFiles  = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);

//    var compilation = CSharpCompilation.Create("",);
//    compilation.
//    foreach (var csFile in csFiles) {
//        var codes = File.ReadAllText(csFile);
//        var tree = CSharpSyntaxTree.ParseText(codes, null, csFile);
//        foreach(var node in tree.) {

//        }
//        Document doc;
//        doc.GetSyntaxRootAsync()
//    }
//}

SyntaxNode TransformContent(string codes, string filePath = "") {
    var tree = CSharpSyntaxTree.ParseText(codes, null, filePath);
    var root = tree.GetRoot();
    PassBase[] passes;

    // collecting phase
    passes = new PassBase[] {
        new ChangeUsingDirectPass(@"Unity\.([^\.]*)","UnityS.$1"),
        new ChangeUsingDeclPass(@"Unity\.([^\.]*)","UnityS.$1"),
        new ReplacePredefTypePass(@"float","sfloat"),
        new ReplaceUserdefTypePass(@"double4x4","float4"),
    };
    foreach (var pass in passes) {
        root = pass.Transform(root);
    }


    // translation phase
    passes = new PassBase[] {
        new ChangeUsingDirectPass(@"Unity\.([^\.]*)","UnityS.$1"),
        new ChangeUsingDeclPass(@"Unity\.([^\.]*)","UnityS.$1"),
        new ReplacePredefTypePass(@"float","sfloat"),
        new ReplaceUserdefTypePass(@"double4x4","float4"),
        new ReplaceNumericLiteralPass(f=>$"sfloat.CreateFor({f})"),
    };
    foreach (var pass in passes) {
        root = pass.Transform(root);
    }


    return root;
}

var root = TransformContent(@"
using Unity;
using Unity.Good;

namespace Unity.ASD{
    class Math{
        float X = 13.01f+1e3 - 1 + 123f;
        float Y = (float)1 + float.E;
        double4x4 Mat4;

    }

}
namespace UnityASD{


}
", "");

Console.WriteLine(root.ToString());