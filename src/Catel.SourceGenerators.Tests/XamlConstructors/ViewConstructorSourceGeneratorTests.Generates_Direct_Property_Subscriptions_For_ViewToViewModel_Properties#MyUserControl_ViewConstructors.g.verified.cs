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

            var viewPropertySelector = GetService<IViewPropertySelector>();
            viewPropertySelector.AddPropertyToSubscribe("Title", typeof(MyUserControl));
            viewPropertySelector.AddPropertyToSubscribe("Count", typeof(MyUserControl));
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "{AssemblyVersion}")]
        static MyUserControl()
        {
            InitializeViewPropertyMappings();
        }

        partial void OnInitializingComponent();

        partial void OnInitializedComponent();

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "{AssemblyVersion}")]
        public MyUserControl(Catel.Windows.Controls.ILogger logger, Catel.Windows.Controls.IUserControlWrapperService service)
            : base(logger, service)
        {
            OnInitializingComponent();
            InitializeComponent();
            OnInitializedComponent();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewConstructors", "{AssemblyVersion}")]
        public MyUserControl()
            : this(GetService<Catel.Windows.Controls.ILogger>(), GetService<Catel.Windows.Controls.IUserControlWrapperService>())
        {
        }

    }
}
