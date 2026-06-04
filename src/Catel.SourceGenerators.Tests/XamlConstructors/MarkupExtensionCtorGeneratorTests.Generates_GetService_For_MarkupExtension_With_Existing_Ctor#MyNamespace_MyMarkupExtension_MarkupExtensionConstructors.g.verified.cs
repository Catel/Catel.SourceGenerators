//HintName: MyNamespace_MyMarkupExtension_MarkupExtensionConstructors.g.cs
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Catel.IoC;

#nullable enable

namespace MyNamespace
{
    partial class MyMarkupExtension
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.MarkupExtensionConstructors", "{AssemblyVersion}")]
        private static T GetService<T>()
            where T : class
        {
            if (Catel.CatelEnvironment.IsInDesignMode)
            {
                return null!;
            }

            return Catel.IoC.IoCContainer.ServiceProvider.GetRequiredService<T>();
        }

    }
}
