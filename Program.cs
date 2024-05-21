// See https://aka.ms/new-console-template for more information
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using FloatSyntaxConv;
using FloatSyntaxConv.Passes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

PassBase[] passes;

unsafe {
    passes = new PassBase[] {
        new ConstantPropagatePass(),
        new DisableDefaultParametersPass(),
        new ReplaceSizeOfPass(new Dictionary<string, string>()
        {
            {"float","uint" },
        }),
        new DisableFieldConstPass(new HashSet<string>{"float"}),
        new ReplaceNamePass(ns => {
            if(ns.StartsWith("Unity.Mathematics")) return "UnityS"+ns[5..];
            if(ns.StartsWith("Unity.Physics")) return "UnityS"+ns[5..];
            return ns;
        }),
        new ReplacePredefinedTypePass(@"float","sfloat"),
        //new ReplacePredefTypePass(@"double","sfloat"),
        new ReplaceUserdefinedTypePass(@"double4x4","float4x4"),
        new ReplaceUserdefinedTypePass(@"double3x3","float3x3"),
        new ReplaceUserdefinedTypePass(@"double4","float4"),
        new ReplaceUserdefinedTypePass(@"double3","float3"),
        new ReplaceNumericLiteralPass(f=>f==1?"sfloat.One":$"sfloat.FromRaw({*(uint*)&f})/*={f}*/"),
        new DisableBlockConstPass( DisableBlockOption.RemoveConstKeywordOnly),
    };
}


#if Develop // 切换到Develop模式调试自定义文本
var contents = new string[]
{
    @"

class Entity{
    fixed byte buffer[sizeof(float) * 3 + 1];
    const float PI = 3.14f;
    void Foo(string name = ""asd"", float f = Entity.PI){
        Foo(name,1f);
        Foo(name,f);
        Foo(name);
    }
    void Foo1(){
        Foo1("""");

    }
    void Foo1(string name = ""asd"", float f = 13){
        Foo1(name,1f);
        Foo1(name,f);
        Foo1(name);
    }

}

",
};

foreach(var node in ConsumeFileContents(contents))
{
    Console.WriteLine(node);
}
void Foo(int a = 13 + 1)
{

}
#else

Parser.Default.ParseArguments<CliOptions>(args).WithParsed(opt => {
    Console.WriteLine($"translating {opt.InputPath} into {opt.OutputPath}...");
    ConsumeDirectorySources(opt.InputPath, opt.OutputPath);
});
#endif

IEnumerable<SyntaxNode> WalkSyntaxTrees(CSharpCompilation compilation) {
    var trees = compilation.SyntaxTrees;
    foreach (var t in compilation.SyntaxTrees) {

        var root = t.GetRoot();
        foreach (var pass in passes) {
            //var model = compilation.GetSemanticModel(root.SyntaxTree, true);
            root = pass.Transform(root, compilation);
        }
        yield return root;
    }
}

IEnumerable<SyntaxNode> ConsumeFileContents(params string[] contents) {
    var compilation = CSharpCompilation.Create("",
        contents.Select(t => CSharpSyntaxTree.ParseText(t, CSharpParseOptions.Default))
    );
    foreach (var node in WalkSyntaxTrees(compilation)) yield return node;
}

IEnumerable<SyntaxTree> GetSyntaxTrees(params string[] csFiles) {
    foreach (var csFile in csFiles) {
        var content = File.ReadAllText(csFile);
        var tree = CSharpSyntaxTree.ParseText(content, null, csFile);
        yield return tree;
    }
}

void ConsumeDirectorySources(string directory, string outdir) {
    var csFiles = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories);
    var compilation = CSharpCompilation.Create("Dummy",
        GetSyntaxTrees(csFiles)
    );
    var nodes = WalkSyntaxTrees(compilation);
    foreach (var node in nodes) {
        var path = node.SyntaxTree.FilePath;
        var relpath = Path.GetRelativePath(directory, path);
        var outPath = Path.Combine(outdir, relpath);
        var outPathDir = Path.GetDirectoryName(outPath);
        if (!Directory.Exists(outPathDir))
            Directory.CreateDirectory(outPathDir);

        File.WriteAllText(outPath, node.ToString());
        Console.WriteLine("output: " + relpath);
    }
}

class CliOptions {

    [Option('i', "input-path", Required = true, HelpText = "The folder path of cs files.")]
    public string InputPath { get; set; }
    [Option('o', "output-path", Required = true, HelpText = "The output path of translated cs files.")]
    public string OutputPath { get; set; }
}

