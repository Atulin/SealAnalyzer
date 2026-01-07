using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using SealAnalyzer.CodeFixes;

namespace SealAnalyzer.Tests;

public class MakeTypeSealedCodeFixProviderTests
{
    [Fact]
    public async Task CodeFix_AddsSealed_ToInternalClass()
    {
        const string code = """
            internal class {|SEAL001:TestClass|}
            {
            }
            """;

        const string fixedCode = """
            internal sealed class TestClass
            {
            }
            """;

        await VerifyCodeFixAsync(code, fixedCode);
    }

    [Fact]
    public async Task CodeFix_AddsSealed_ToPrivateClass()
    {
        const string code = """
            public class Container
            {
                private class {|SEAL001:TestClass|}
                {
                }
            }
            """;

        const string fixedCode = """
            public class Container
            {
                private sealed class TestClass
                {
                }
            }
            """;

        await VerifyCodeFixAsync(code, fixedCode);
    }

    [Fact]
    public async Task CodeFix_AddsSealed_ToPublicClass_WithOptIn()
    {
        const string code = """
            using AutoSeal;
            
            [assembly: SealPublicClasses]

            public class {|SEAL001:TestClass|}
            {
            }
            """;

        const string fixedCode = """
            using AutoSeal;
            
            [assembly: SealPublicClasses]

            public sealed class TestClass
            {
            }
            """;

        await VerifyCodeFixAsync(code, fixedCode, includeGenerator: true);
    }

    [Fact]
    public async Task CodeFix_AddsSealed_AfterAccessModifiers()
    {
        const string code = """
            public class Container
            {
                protected internal class {|SEAL001:TestClass|}
                {
                }
            }
            """;

        const string fixedCode = """
            public class Container
            {
                protected internal sealed class TestClass
                {
                }
            }
            """;

        await VerifyCodeFixAsync(code, fixedCode);
    }

    [Fact]
    public async Task CodeFix_AddsSealed_BeforePartial()
    {
        const string code = """
            internal partial class {|SEAL001:TestClass|}
            {
            }
            """;

        const string fixedCode = """
            internal sealed partial class TestClass
            {
            }
            """;

        await VerifyCodeFixAsync(code, fixedCode);
    }

    [Fact]
    public async Task CodeFix_AddsSealed_ToRecord()
    {
        const string code = """
            internal record {|SEAL001:TestRecord|}
            {
            }
            """;

        const string fixedCode = """
            internal sealed record TestRecord
            {
            }
            """;

        await VerifyCodeFixAsync(code, fixedCode);
    }

    [Fact]
    public async Task CodeFix_AddsSealed_ToRecordClass()
    {
        const string code = """
            internal record class {|SEAL001:TestRecord|}
            {
            }
            """;

        const string fixedCode = """
            internal sealed record class TestRecord
            {
            }
            """;

        await VerifyCodeFixAsync(code, fixedCode);
    }

    [Fact]
    public async Task CodeFix_PreservesComments()
    {
        const string code = """
            /// <summary>
            /// Test class documentation
            /// </summary>
            internal class {|SEAL001:TestClass|}
            {
            }
            """;

        const string fixedCode = """
            /// <summary>
            /// Test class documentation
            /// </summary>
            internal sealed class TestClass
            {
            }
            """;

        await VerifyCodeFixAsync(code, fixedCode);
    }

    [Fact]
    public async Task CodeFix_FixesMultipleClasses()
    {
        const string code = """
            internal class {|SEAL001:ClassOne|}
            {
            }

            internal class {|SEAL001:ClassTwo|}
            {
            }
            """;

        const string fixedCode = """
            internal sealed class ClassOne
            {
            }

            internal sealed class ClassTwo
            {
            }
            """;

        await VerifyCodeFixAsync(code, fixedCode);
    }

    [Fact]
    public async Task CodeFix_WorksWithComplexModifiers()
    {
        const string code = """
            public class Container
            {
                private partial class {|SEAL001:TestClass|}
                {
                }
            }
            """;

        const string fixedCode = """
            public class Container
            {
                private sealed partial class TestClass
                {
                }
            }
            """;

        await VerifyCodeFixAsync(code, fixedCode);
    }

    private static async Task VerifyCodeFixAsync(string source, string fixedSource, bool includeGenerator = false)
    {
        var test = new CSharpCodeFixTest<MakeTypeSealedAnalyzer, MakeTypeSealedCodeFixProvider, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source }
            },
            FixedState =
            {
                Sources = { fixedSource }
            }
        };

        if (includeGenerator)
        {
            test.TestState.AdditionalReferences.Add(typeof(SealPublicClassesGenerator).Assembly);
            test.FixedState.AdditionalReferences.Add(typeof(SealPublicClassesGenerator).Assembly);
        }

        await test.RunAsync();
    }
}