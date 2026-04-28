//HintName: MyConverter_BehaviorConstructors.g.cs
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Catel.IoC;

#nullable enable

namespace MyNamespace
{
    partial class MyConverter
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ValueConverterConstructors", "1.0.0.0")]
        private static T GetService<T>()
            where T : class
        {
            if (Catel.CatelEnvironment.IsInDesignMode)
            {
                return null!;
            }

            return Catel.IoC.IoCContainer.ServiceProvider.GetRequiredService<T>();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ValueConverterConstructors", "1.0.0.0")]
        public MyConverter()
            : this(GetService<ILanguageService>())
        {
        }
    }
}
