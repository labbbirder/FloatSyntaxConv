// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FloatSyntaxConv;
using FloatSyntaxConv.Passes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

Console.WriteLine("Hello, World!");


IEnumerable<SyntaxTree> GetSyntaxTrees(params string[] csFiles)
{
    foreach(var csFile in csFiles)
    {
        var content = File.ReadAllText(csFile);
        var tree = CSharpSyntaxTree.ParseText(content, null, csFile);
        yield return tree;
    }
}

void VisitDirectory(string directory,string output) {
    var csFiles  = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
    var compilation = CSharpCompilation.Create("",
        GetSyntaxTrees(csFiles)
    );
    var trees = compilation.SyntaxTrees;
    foreach(var tree in trees) {

    }
}

IEnumerable<SyntaxNode> TestGroup(params string[] contents)
{
    var compilation = CSharpCompilation.Create("",
        contents.Select(t => CSharpSyntaxTree.ParseText(t))
    ) ;
    foreach(var t in compilation.SyntaxTrees)
    {
        var model = compilation.GetSemanticModel(t, true);
        PassBase[] passes;

        //// collecting phase
        passes = new PassBase[] {
            new ChangeUsingDirectPass(@"Unity\.([^\.]*)","UnityS.$1"),
            new ChangeUsingDeclPass(@"Unity\.([^\.]*)","UnityS.$1"),
            new ReplacePredefTypePass(@"float","sfloat"),
            new ReplaceUserdefTypePass(@"double4x4","float4"),
            new ReplaceNumericLiteralPass(f=>$"sfloat.CreateFor({f})"),
            new DisableBlockConstPass( DisableBlockOption.RemoveConstKeywordOnly),
        };
        var root = t.GetRoot();
        foreach (var pass in passes)
        {
            root = pass.Transform(root,model);
        }
        yield return root;
    }
}

SyntaxNode TransformContent(string codes, string filePath = "") {
    var tree = CSharpSyntaxTree.ParseText(codes, null, filePath);
    var root = tree.GetRoot();
    PassBase[] passes;

    //// collecting phase
    //passes = new PassBase[] {
    //    new ChangeUsingDirectPass(@"Unity\.([^\.]*)","UnityS.$1"),
    //    new ChangeUsingDeclPass(@"Unity\.([^\.]*)","UnityS.$1"),
    //    new ReplacePredefTypePass(@"float","sfloat"),
    //    new ReplaceUserdefTypePass(@"double4x4","float4"),
    //    new ReplaceNumericLiteralPass(f=>$"sfloat.CreateFor({f})"),
    //    new DisableBlockConstPass( DisableBlockOption.RemoveConstKeywordOnly),
    //};
    //foreach (var pass in passes) {
    //    root = pass.Transform(root);
    //}


    //// translation phase
    //passes = new PassBase[] {
    //    new ChangeUsingDirectPass(@"Unity\.([^\.]*)","UnityS.$1"),
    //    new ChangeUsingDeclPass(@"Unity\.([^\.]*)","UnityS.$1"),
    //    new ReplacePredefTypePass(@"float","sfloat"),
    //    new ReplaceUserdefTypePass(@"double4x4","float4"),
    //    new ReplaceNumericLiteralPass(f=>$"sfloat.CreateFor({f})"),
    //};
    //foreach (var pass in passes) {
    //    root = pass.Transform(root);
    //}


    return root;
}

const string inputPath = "/Users/bbbirder/Downloads/unity-deterministic-physics-master/UnityPhysicsPackage";
const string outputPath = "/Users/bbbirder/Downloads/unity-deterministic-physics-master/Assets/Scripts/Physics";

var root = TestGroup(@"
using Unity;
using Unity.Good;

namespace Unity.ASD{
    class Math{
        float X = 13.01f+1e3 - 1 + 123f;
        float Y = (float)1 + float.E;
        double4x4 Mat4;
        void Foo(){
            const float pi = 3.1415926f;
        }
        void Test(){
            Bar.Foo();
        }

    }

}
namespace UnityASD{


}
",
@"
public class Bar{
    public static void Foo(){

    }
}

");


foreach(var n in root)
{
    Console.WriteLine(n.ToString());

}

