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
            : this(IoCContainer.ServiceProvider.GetRequiredService<MyNamespace.ILogger<MyNamespace.MyUserControl>>(), IoCContainer.ServiceProvider.GetRequiredService<MyNamespace.IUserControlWrapperService>())
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
