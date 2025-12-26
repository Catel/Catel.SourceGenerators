//HintName: MyUserControl_UserControlConstructors.g.cs
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Catel.IoC;

#nullable enable

namespace MyNamespace
{
    partial class MyUserControl
    {
        private static T GetService<T>()
            where T : class
        {
            if (Catel.CatelEnvironment.IsInDesignMode)
            {
                return null!;
            }

            return Catel.IoC.IoCContainer.ServiceProvider.GetRequiredService<T>();
        }

        partial void OnInitializingComponent();

        partial void OnInitializedComponent();

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.UserControlConstructors", "1.0.0.0")]
        [ActivatorUtilitiesConstructor]
        public MyUserControl(MyNamespace.ILogger<MyNamespace.MyUserControl> logger, MyNamespace.IUserControlWrapperService userControlWrapperService)
            : base(logger, userControlWrapperService)
        {
            OnInitializingComponent();
            InitializeComponent();
            OnInitializedComponent();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.UserControlConstructors", "1.0.0.0")]
        public MyUserControl()
            : this(GetService<MyNamespace.ILogger<MyNamespace.MyUserControl>>(), GetService<MyNamespace.IUserControlWrapperService>())
        {
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.UserControlConstructors", "1.0.0.0")]
        public MyUserControl(Catel.MVVM.IViewModel? viewModel, MyNamespace.ILogger<MyNamespace.MyUserControl> logger, MyNamespace.IUserControlWrapperService userControlWrapperService)
            : base(viewModel, logger, userControlWrapperService)
        {
            OnInitializingComponent();
            InitializeComponent();
            OnInitializedComponent();
        }

    }
}
