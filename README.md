# CSharp浮点语法转换工具

此工具用于转换已有的浮点代码库，转换成可用于帧同步项目的孪生变体。

比如，考虑如下代码：

```csharp
namespace com.bbbirder.Simulation{

    public class Math{
        const float PI = 3.1415926f;

        public float Div(float a, float b){
            return a/b;
        }
    }
}
```
转换后变为以下形式：

```csharp
namespace com.bbbirder.Simulation{

    public class Math{
        static sfloat PI = sfloat.FromRaw(0x40490fdb);

        public sfloat Div(sfloat a, sfloat b){
            return a/b;
        }
    }
}
```
## CLI Usage
构建后，通过命令行运行；或者直接修改运行`Test.bat`

命令行参数如下：
```bash
FloatSyntaxConv 1.0.0+816f22ed4d5839a40ee5aa01fc78b70d72ec53e0
Copyright (C) 2024 bbbirder

  -i, --input-path     Required. The folder path of cs files.

  -o, --output-path    Required. The output path of translated cs files.

  --help               Display this help screen.

  --version            Display version information.
```

## Major Passes

为快速了解原理及思想，这里介绍几个主要的pass。

首先，我们希望替换代码中所有的：
* 浮点成员声明
* 浮点参数声明
* 浮点局部定义
* 浮点类型的静态成员
* 显示浮点类型转换

则，我们使用`new ReplacePredefinedTypePass(@"float","sfloat")`实现。

下一步，代码中会有浮点字面量，如：`3.14f`，我们使用`new ReplaceNumericLiteralPass(f=>$"sfloat.FromRaw({*(uint*)&f})"),`实现。

> 浮点字面量是确定性的吗？
> 
> 答案是复杂的。这里以sfloat的实现为例展开讨论。
> 
> 从浮点数值角度分析：如果浮点经过IEEE754编码不会截断，则是确定性的，否则，需从编译环境角度继续分析。
> 
> 从编译环境角度分析：如果字面量经过固定的计算机编译，并以二进制的形式存储在PE文件中，那么当CPU从总线读取浮点数据后，原封不动的转为sfloat，则整个过程是确定性的，包括解释执行、JIT执行、AOT编译本地代码执行；如果使用不同的计算机编译，或者将il2cpp的中间过程代码（cpp）传输到其他计算机二次编译，则结果是非确定性的。
>
> 如果不是c#代码中的浮点，而是文本脚本、文本数据，则结果是非确定性的
>
> 综上，我们可以粗暴地认为浮点字面量一律是非确定性的。

经过多道Pass，绝大多数的软浮点转换操作都已完成，后续只需要手动做一些边角修改，或者针对边角修改实现新的pass以实现完全自动化。

## More Details

使用Roslyn分析代码，结构上与链接器的写法类似，使用多道Pass来visit代码。

可以自己实现新的Pass

如果需要对接其他软浮点实现，则可参考Program.cs调整Pass参数
