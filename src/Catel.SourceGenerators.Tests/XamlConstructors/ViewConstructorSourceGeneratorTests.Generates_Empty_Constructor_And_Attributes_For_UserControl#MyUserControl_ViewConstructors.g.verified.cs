//HintName: MyUserControl_ViewConstructors.g.cs
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Catel;
using Catel.IoC;
using Catel.MVVM.Views;

#nullable enable

namespace MyNamespace
{
    partial class MyUserControl
    {
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "1.0.0.0")]
        private static T GetService<T>()
            where T : class
        {
            if (Catel.CatelEnvironment.IsInDesignMode)
            {
                return null!;
            }

            return Catel.IoC.IoCContainer.ServiceProvider.GetRequiredService<T>();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "1.0.0.0")]
        private static void InitializeViewPropertyMappings()
        {
            if (CatelEnvironment.IsInDesignMode)
            {
                return;
            }

            typeof(MyUserControl).AutoDetectViewPropertiesToSubscribe(IoCContainer.ServiceProvider.GetRequiredService<IViewPropertySelector>());
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "1.0.0.0")]
        static MyUserControl()
        {
            InitializeViewPropertyMappings();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "1.0.0.0")]
        public MyUserControl()
            : this(GetService<MyNamespace.ILogger<MyNamespace.MyUserControl>>(), GetService<MyNamespace.IUserControlWrapperService>())
        {
        }

    }
}
