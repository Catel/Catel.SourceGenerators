//HintName: MyUserControl_UserControlConstructors.g.cs
using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Catel.IoC;

namespace MyNamespace
{
    partial class MyUserControl
    {
        [CompilerGenerated]
        public MyUserControl()
            : this(IoCContainer.ServiceProvider.GetRequiredService<MyNamespace.ILogger<MyNamespace.MyUserControl>>(), IoCContainer.ServiceProvider.GetRequiredService<MyNamespace.IUserControlWrapperService>())
        {
        }
    }
}
