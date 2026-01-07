using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace SealAnalyzer.Tests;

public class SealPublicClassesGeneratorTests
{
    [Fact]
    public async Task Generator_ProducesAttributeSource()
    {
        const string source = """
            // Empty compilation to test generator
            """;

        const string generatedSource = """
            using System;

            namespace AutoSeal
            {
                [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
                internal sealed class SealPublicClassesAttribute : Attribute 
                {
                }
            }
            """;

        await VerifyGeneratorAsync(source, generatedSource);
    }

    [Fact]
    public async Task Generator_AllowsAttributeUsage()
    {
        const string source = """
            [assembly: AutoSeal.SealPublicClasses]

            public class TestClass
            {
            }
            """;

        const string generatedSource = """
            using System;

            namespace AutoSeal
            {
                [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
                internal sealed class SealPublicClassesAttribute : Attribute 
                {
                }
            }
            """;

        await VerifyGeneratorAsync(source, generatedSource);
    }

    [Fact]
    public void Generator_AttributeIsInternal()
    {
        const string generatedSource = """
            using System;

            namespace AutoSeal
            {
                [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
                internal sealed class SealPublicClassesAttribute : Attribute 
                {
                }
            }
            """;

        var compilation = CSharpCompilation.Create("TestCompilation")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(generatedSource));

        var attributeSymbol = compilation.GetTypeByMetadataName("AutoSeal.SealPublicClassesAttribute");
        Assert.NotNull(attributeSymbol);
        Assert.Equal(Accessibility.Internal, attributeSymbol.DeclaredAccessibility);
    }

    [Fact]
    public void Generator_AttributeIsSealed()
    {
        const string generatedSource = """
            using System;

            namespace AutoSeal
            {
                [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
                internal sealed class SealPublicClassesAttribute : Attribute 
                {
                }
            }
            """;

        var compilation = CSharpCompilation.Create("TestCompilation")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(generatedSource));

        var attributeSymbol = compilation.GetTypeByMetadataName("AutoSeal.SealPublicClassesAttribute");
        Assert.NotNull(attributeSymbol);
        Assert.True(attributeSymbol.IsSealed);
    }

    [Fact]
    public void Generator_AttributeInheritsFromAttribute()
    {
        const string generatedSource = """
            using System;

            namespace AutoSeal
            {
                [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
                internal sealed class SealPublicClassesAttribute : Attribute 
                {
                }
            }
            """;

        var compilation = CSharpCompilation.Create("TestCompilation")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(generatedSource));

        var attributeSymbol = compilation.GetTypeByMetadataName("AutoSeal.SealPublicClassesAttribute");
        Assert.NotNull(attributeSymbol);
        Assert.NotNull(attributeSymbol.BaseType);
        Assert.Equal("System.Attribute", attributeSymbol.BaseType.ToDisplayString());
    }

    [Fact]
    public void Generator_AttributeHasCorrectUsage()
    {
        const string generatedSource = """
            using System;

            namespace AutoSeal
            {
                [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
                internal sealed class SealPublicClassesAttribute : Attribute 
                {
                }
            }
            """;

        var compilation = CSharpCompilation.Create("TestCompilation")
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(generatedSource));

        var attributeSymbol = compilation.GetTypeByMetadataName("AutoSeal.SealPublicClassesAttribute");
        Assert.NotNull(attributeSymbol);
        
        var attributeUsageAttribute = attributeSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == "AttributeUsageAttribute");
        
        Assert.NotNull(attributeUsageAttribute);
    }

    private static async Task VerifyGeneratorAsync(string source, string generatedSource)
    {
        var test = new CSharpSourceGeneratorTest<SealPublicClassesGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source },
                GeneratedSources =
                {
                    (typeof(SealPublicClassesGenerator), "SealPublicClassesAttribute.g.cs", generatedSource)
                }
            }
        };

        await test.RunAsync();
    }
}