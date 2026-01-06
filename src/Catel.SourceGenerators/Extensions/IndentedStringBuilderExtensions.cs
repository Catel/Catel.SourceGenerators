namespace Catel.SourceGenerators
{
    using System.CodeDom.Compiler;

    internal static class IndentedStringBuilderExtensions
    {
        internal static readonly string GeneratorVersion = typeof(IndentedStringBuilderExtensions).Assembly.GetName().Version.ToString();

        public static void StartBlock(this IndentedStringBuilder writer)
        {
            writer.AppendLine("{");
            writer.IncrementIndent();
        }

        public static void EndBlock(this IndentedStringBuilder writer)
        {
            writer.DecrementIndent();
            writer.AppendLine("}");
        }

        public static void AppendGeneratedCodeAttribute(this IndentedStringBuilder writer, string generatorName)
        {
            writer.AppendLine($"""[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.{generatorName}", "{GeneratorVersion}")]""");
        }

        public static void AppendResolveServiceMethod(this IndentedStringBuilder writer, string generatorName)
        {
            writer.AppendGeneratedCodeAttribute(generatorName);
            writer.AppendLine($"private static T GetService<T>()");
            writer.AppendLine($"    where T : class");
            writer.StartBlock();
            writer.AppendLine("if (Catel.CatelEnvironment.IsInDesignMode)");
            writer.StartBlock();
            writer.AppendLine("return null!;");
            writer.EndBlock();
            writer.AppendLine();
            writer.AppendLine("return Catel.IoC.IoCContainer.ServiceProvider.GetRequiredService<T>();");
            writer.EndBlock();
            writer.AppendLine();
        }
    }
}
