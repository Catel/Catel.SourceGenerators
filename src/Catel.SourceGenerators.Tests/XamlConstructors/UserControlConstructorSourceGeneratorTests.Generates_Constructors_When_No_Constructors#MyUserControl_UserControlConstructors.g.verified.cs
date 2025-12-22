//HintName: MyUserControl_UserControlConstructors.g.cs
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Catel.IoC;

namespace MyNamespace
{
    partial class MyUserControl
    {
        partial void OnInitializingComponent();

        partial void OnInitializedComponent();

        [CompilerGenerated]
        [ActivatorUtilitiesConstructor]
        public MyUserControl(MyNamespace.ILogger<MyNamespace.MyUserControl> logger, MyNamespace.IUserControlWrapperService userControlWrapperService)
            : base(logger, userControlWrapperService)
        {
            OnInitializingComponent();
            InitializeComponent();
            OnInitializedComponent();
        }

        [CompilerGenerated]
        public MyUserControl()
            : this(IoCContainer.ServiceProvider.GetRequiredService<MyNamespace.ILogger<MyNamespace.MyUserControl>>(), IoCContainer.ServiceProvider.GetRequiredService<MyNamespace.IUserControlWrapperService>())
        {
        }

    }
}
