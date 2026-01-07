using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SealAnalyzer
{
	[Generator]
	public sealed class SealPublicClassesGenerator : IIncrementalGenerator
	{
		public const string Namespace = "AutoSeal";
		public const string AttributeName = "SealPublicClassesAttribute";
		
		private readonly string _source = $@"
using System;

namespace { Namespace}
{{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    internal sealed class { AttributeName } : Attribute 
    {{
    }}
}}";

		public void Initialize(IncrementalGeneratorInitializationContext context)
		{
			// We simply add the source code for the attribute to the compilation
			context.RegisterPostInitializationOutput(ctx => {
				ctx.AddSource("SealPublicClassesAttribute.g.cs", SourceText.From(_source, Encoding.UTF8));
			});
		}
	}
}