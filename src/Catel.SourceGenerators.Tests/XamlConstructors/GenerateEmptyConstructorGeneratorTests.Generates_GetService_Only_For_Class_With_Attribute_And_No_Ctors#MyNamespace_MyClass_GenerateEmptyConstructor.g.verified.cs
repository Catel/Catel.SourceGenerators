//HintName: MyNamespace_MyClass_GenerateEmptyConstructor.g.cs
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Catel.IoC;

#nullable enable

namespace MyNamespace
{
    partial class MyClass
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.GenerateEmptyConstructor", "{AssemblyVersion}")]
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
