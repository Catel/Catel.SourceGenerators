//HintName: MyUserControl_XamlConstructors.g.cs
using System;
using System.Runtime.CompilerServices;
using Catel.IoC;

namespace MyNamespace
{
    partial class MyUserControl
    {
        [CompilerGenerated]
        public MyUserControl()
            : this(IoCContainer.Provider.GetRequiredService<MyNamespace.ILogger<MyNamespace.MyUserControl>>(), IoCContainer.Provider.GetRequiredService<MyNamespace.IUserControlWrapperService>())
        {
        }
    }
}
