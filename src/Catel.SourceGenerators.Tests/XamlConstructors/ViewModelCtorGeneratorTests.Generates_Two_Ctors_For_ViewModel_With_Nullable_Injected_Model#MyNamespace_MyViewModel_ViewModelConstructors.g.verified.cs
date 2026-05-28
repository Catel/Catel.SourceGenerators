//HintName: MyNamespace_MyViewModel_ViewModelConstructors.g.cs
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
        public MyViewModel(MyNamespace.MyModel model, System.IServiceProvider serviceProvider, MyNamespace.IMyService1 myService1, MyNamespace.IMyService2 myService2)
            : base(serviceProvider)
        {
            Model = model;
            _myService1 = myService1;
            _myService2 = myService2;
            OnConstructing();
            OnConstructed();
        }

        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Catel.ViewModelConstructors", "{AssemblyVersion}")]
        public MyViewModel(System.IServiceProvider serviceProvider, MyNamespace.IMyService1 myService1, MyNamespace.IMyService2 myService2)
            : base(serviceProvider)
        {
            _myService1 = myService1;
            _myService2 = myService2;
            OnConstructing();
            OnConstructed();
        }
    }
}
