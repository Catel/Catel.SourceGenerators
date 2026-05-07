//HintName: MyBehavior_BehaviorConstructors.g.cs
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Catel.IoC;

#nullable enable

namespace MyNamespace
{
    partial class MyBehavior
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.BehaviorConstructors", "{AssemblyVersion}")]
        private static T GetService<T>()
            where T : class
        {
            if (Catel.CatelEnvironment.IsInDesignMode)
            {
                return null!;
            }

            return Catel.IoC.IoCContainer.ServiceProvider.GetRequiredService<T>();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.BehaviorConstructors", "{AssemblyVersion}")]
        public MyBehavior(MyNamespace.IMyService1 myService1, MyNamespace.IMyService2 myService2)
        {
            _myService1 = myService1;
            _myService2 = myService2;
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.BehaviorConstructors", "{AssemblyVersion}")]
        public MyBehavior()
            : this(GetService<MyNamespace.IMyService1>(), GetService<MyNamespace.IMyService2>())
        {
        }
    }
}
