//HintName: MyMarkupExtension_MarkupExtensionConstructors.g.cs
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

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.MarkupExtensionConstructors", "{AssemblyVersion}")]
        public MyMarkupExtension(MyNamespace.IMyService1 myService1, MyNamespace.IMyService2 myService2)
        {
            _myService1 = myService1;
            _myService2 = myService2;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.MarkupExtensionConstructors", "{AssemblyVersion}")]
        public MyMarkupExtension()
            : this(GetService<MyNamespace.IMyService1>(), GetService<MyNamespace.IMyService2>())
        {
        }
    }
}
