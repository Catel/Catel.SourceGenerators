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
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "{AssemblyVersion}")]
        private static T GetService<T>()
            where T : class
        {
            if (Catel.CatelEnvironment.IsInDesignMode)
            {
                return null!;
            }

            return Catel.IoC.IoCContainer.ServiceProvider.GetRequiredService<T>();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "{AssemblyVersion}")]
        private static void InitializeViewPropertyMappings()
        {
            if (CatelEnvironment.IsInDesignMode)
            {
                return;
            }
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "{AssemblyVersion}")]
        static MyUserControl()
        {
            InitializeViewPropertyMappings();
        }

        partial void OnInitializingComponent();

        partial void OnInitializedComponent();

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "{AssemblyVersion}")]
        public MyUserControl(MyNamespace.ILogger<MyNamespace.MyUserControl> logger, MyNamespace.IUserControlWrapperService userControlWrapperService, MyNamespace.IMyService1 myService1, MyNamespace.IMyService2 myService2)
            : base(logger, userControlWrapperService)
        {
            _myService1 = myService1;
            _myService2 = myService2;
            OnInitializingComponent();
            InitializeComponent();
            OnInitializedComponent();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "{AssemblyVersion}")]
        public MyUserControl()
            : this(GetService<MyNamespace.ILogger<MyNamespace.MyUserControl>>(), GetService<MyNamespace.IUserControlWrapperService>(), GetService<MyNamespace.IMyService1>(), GetService<MyNamespace.IMyService2>())
        {
        }

    }
}
