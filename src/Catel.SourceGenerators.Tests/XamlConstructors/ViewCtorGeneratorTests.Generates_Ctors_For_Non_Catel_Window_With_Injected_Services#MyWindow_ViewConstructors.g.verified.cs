//HintName: MyWindow_ViewConstructors.g.cs
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Catel;
using Catel.IoC;
using Catel.MVVM.Views;

#nullable enable

namespace MyNamespace
{
    partial class MyWindow
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


        partial void OnInitializingComponent();

        partial void OnInitializedComponent();

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "{AssemblyVersion}")]
        public MyWindow(MyNamespace.IMyService myService)
            : base()
        {
            _myService = myService;
            OnInitializingComponent();
            InitializeComponent();
            OnInitializedComponent();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "{AssemblyVersion}")]
        public MyWindow()
            : this(GetService<MyNamespace.IMyService>())
        {
        }

    }
}
