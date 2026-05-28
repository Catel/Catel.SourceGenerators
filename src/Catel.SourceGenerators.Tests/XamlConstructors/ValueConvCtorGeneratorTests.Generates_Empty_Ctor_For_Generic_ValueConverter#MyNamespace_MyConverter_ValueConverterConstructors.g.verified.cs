//HintName: MyNamespace_MyConverter_ValueConverterConstructors.g.cs
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Catel.IoC;

#nullable enable

namespace MyNamespace
{
    partial class MyConverter
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ValueConverterConstructors", "{AssemblyVersion}")]
        private static T GetService<T>()
            where T : class
        {
            if (Catel.CatelEnvironment.IsInDesignMode)
            {
                return null!;
            }

            return Catel.IoC.IoCContainer.ServiceProvider.GetRequiredService<T>();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ValueConverterConstructors", "{AssemblyVersion}")]
        public MyConverter()
            : this(GetService<ILanguageService>())
        {
        }
    }
}
