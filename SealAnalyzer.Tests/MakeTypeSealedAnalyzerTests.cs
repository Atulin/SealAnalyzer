using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace SealAnalyzer.Tests;

public class MakeTypeSealedAnalyzerTests
{
    [Fact]
    public async Task NoDiagnostic_ForSealedClass()
    {
        const string code = """
            public sealed class TestClass
            {
            }
            """;

        await VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task NoDiagnostic_ForAbstractClass()
    {
        const string code = """
            public abstract class TestClass
            {
            }
            """;

        await VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task NoDiagnostic_ForStaticClass()
    {
        const string code = """
            public static class TestClass
            {
            }
            """;

        await VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task Diagnostic_ForPrivateClass()
    {
        const string code = """
            public class Container
            {
                private class {|SEAL001:TestClass|}
                {
                }
            }
            """;

        await VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task Diagnostic_ForInternalClass()
    {
        const string code = """
            internal class {|SEAL001:TestClass|}
            {
            }
            """;

        await VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task NoDiagnostic_ForPublicClass_WithoutOptIn()
    {
        const string code = """
            public class TestClass
            {
            }
            """;

        await VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task Diagnostic_ForPublicClass_WithOptIn()
    {
        const string code = """
            using AutoSeal;
            
            [assembly: SealPublicClasses]

            public class {|SEAL001:TestClass|}
            {
            }
            """;

        await VerifyAnalyzerAsync(code, includeGenerator: true);
    }

    [Fact]
    public async Task Diagnostic_ForRecord()
    {
        const string code = """
            internal record {|SEAL001:TestRecord|}
            {
            }
            """;

        await VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task Diagnostic_ForRecordClass()
    {
        const string code = """
            internal record class {|SEAL001:TestRecord|}
            {
            }
            """;

        await VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task NoDiagnostic_ForRecordStruct()
    {
        const string code = """
            internal record struct TestRecord
            {
            }
            """;

        await VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task Diagnostic_ForMultipleClasses()
    {
        const string code = """
            internal class {|SEAL001:ClassOne|}
            {
            }

            internal class {|SEAL001:ClassTwo|}
            {
            }
            """;

        await VerifyAnalyzerAsync(code);
    }

    [Fact]
    public async Task Diagnostic_ForNestedPrivateClass()
    {
        const string code = """
            public class Container
            {
                private class {|SEAL001:Inner|}
                {
                    private class {|SEAL001:DeepInner|}
                    {
                    }
                }
            }
            """;

        await VerifyAnalyzerAsync(code);
    }

    private static async Task VerifyAnalyzerAsync(string source, bool includeGenerator = false)
    {
        var test = new CSharpAnalyzerTest<MakeTypeSealedAnalyzer, DefaultVerifier>
        {
            TestState =
            {
                Sources = { source }
            }
        };

        if (includeGenerator)
        {
            test.TestState.AdditionalReferences.Add(typeof(SealPublicClassesGenerator).Assembly);
        }

        await test.RunAsync();
    }
}