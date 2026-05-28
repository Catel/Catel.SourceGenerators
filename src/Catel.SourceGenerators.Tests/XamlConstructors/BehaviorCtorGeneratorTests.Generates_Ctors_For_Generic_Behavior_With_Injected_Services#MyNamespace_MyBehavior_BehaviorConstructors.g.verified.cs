//HintName: MyNamespace_MyBehavior_BehaviorConstructors.g.cs
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Catel.IoC;

#nullable enable

namespace MyNamespace
{
    partial class MyBehavior<TControl, TSettings>
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

        partial void OnConstructing();

        partial void OnConstructed();

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.BehaviorConstructors", "{AssemblyVersion}")]
        public MyBehavior(MyNamespace.IMyService1 myService1)
        {
            _myService1 = myService1;
            OnConstructing();
            OnConstructed();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.BehaviorConstructors", "{AssemblyVersion}")]
        public MyBehavior()
            : this(GetService<MyNamespace.IMyService1>())
        {
        }
    }
}
