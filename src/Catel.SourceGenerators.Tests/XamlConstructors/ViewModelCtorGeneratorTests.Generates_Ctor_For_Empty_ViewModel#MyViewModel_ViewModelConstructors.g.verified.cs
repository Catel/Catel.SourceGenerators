//HintName: MyViewModel_ViewModelConstructors.g.cs
using System;
using System.Runtime.CompilerServices;

#nullable enable

namespace MyNamespace
{
    partial class MyViewModel
    {
        partial void OnConstructing();

        partial void OnConstructed();

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewModelConstructors", "{AssemblyVersion}")]
        public MyViewModel(System.IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            OnConstructing();
            OnConstructed();
        }
    }
}
