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

## More Details

使用Roslyn分析代码，结构上与链接器的写法类似，使用多道Pass来visit代码。

可以自己实现新的Pass

如果需要对接其他软浮点实现，则可参考Program.cs调整Pass参数