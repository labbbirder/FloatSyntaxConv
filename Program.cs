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
        new DisableDefaultParametersPass(),
        new DisableFieldConstPass(new HashSet<string>{"float"}),
        new ChangeUsingDirectPass(ns => {
            if(ns.StartsWith("Unity.Mathematics")) return "UnityS"+ns[5..];
            if(ns.StartsWith("Unity.Physics")) return "UnityS"+ns[5..];
            return ns;
        }),
        //new ChangeUsingDeclPass(@"Unity\.Mathematics","UnityS.Mathematics"),
        //new ChangeUsingDeclPass(@"Unity\.Physics","UnityS.Physics"),
        new ReplacePredefTypePass(@"float","sfloat"),
        new ReplaceUserdefTypePass(@"double4x4","float4"),
        new ReplaceNumericLiteralPass(f=>$"sfloat.FromRaw({*(uint*)&f})"),
        new DisableBlockConstPass( DisableBlockOption.RemoveConstKeywordOnly),
    };
}

Parser.Default.ParseArguments<CliOptions>(args).WithParsed(opt => {
    Console.WriteLine($"translating {opt.InputPath}...");
    ConsumeDirectorySources(opt.InputPath, opt.OutputPath);
});

IEnumerable<SyntaxNode> WalkSyntaxTrees(CSharpCompilation compilation) {
    var trees = compilation.SyntaxTrees;

    foreach (var t in compilation.SyntaxTrees) {

        var root = t.GetRoot();
        foreach (var pass in passes) {
            var model = compilation.GetSemanticModel(t, true);
            root = pass.Transform(root, model);
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
        Console.WriteLine("output: " + outPath);
    }
}

class CliOptions {

    [Option('i', "input-path", Required = true, HelpText = "The folder path of cs files.")]
    public string InputPath { get; set; }
    [Option('o', "output-path", Required = true, HelpText = "The output path of translated cs files.")]
    public string OutputPath { get; set; }
}

