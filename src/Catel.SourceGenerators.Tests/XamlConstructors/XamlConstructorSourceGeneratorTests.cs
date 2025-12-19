namespace Catel.SourceGenerators.Tests.XamlConstructors
{
    using Catel.SourceGenerators.XamlConstructors;
    using Microsoft.CodeAnalysis.CSharp.Testing;
    using Microsoft.CodeAnalysis.Testing;
    using NUnit.Framework;
    using System.Threading.Tasks;

    [TestFixture]
    public class XamlConstructorSourceGeneratorTests
    {
        [Test]
        public async Task Generates_Empty_Constructor_And_Attributes_For_UserControl()
        {
            var userControlSource = @"
using System.Windows.Controls;
using Catel.IoC;

namespace MyNamespace
{
    public partial class MyUserControl : System.Windows.Controls.UserControl
    {
        public MyUserControl(ILogger<MyUserControl> logger, IUserControlWrapperService userControlWrapperService)
            : base(logger, userControlWrapperService)
        {
            InitializeComponent();
        }
    }

    public interface ILogger<T> {}
    public interface IUserControlWrapperService {}
}
";

            var iocContainerSource = @"
namespace Catel.IoC
{
    public static class IoCContainer
    {
        public static ProviderType Provider { get; } = new ProviderType();
        public class ProviderType
        {
            public T GetRequiredService<T>() => default(T);
        }
    }
}
";

            var test = new CSharpSourceGeneratorTest<XamlConstructorSourceGenerator, DefaultVerifier>
            {
                TestState =
                {
                    Sources = { userControlSource, iocContainerSource }
                }
            };

            test.TestState.GeneratedSources.Add(
                (typeof(XamlConstructorSourceGenerator),
                "MyUserControl_XamlConstructors.g.cs",
    @"
using System;
using System.Runtime.CompilerServices;
using Catel.IoC;

namespace MyNamespace
{
    partial class MyUserControl
    {
        [CompilerGenerated]
        public MyUserControl()
            : this(IoCContainer.Provider.GetRequiredService<ILogger<MyUserControl>>(), IoCContainer.Provider.GetRequiredService<IUserControlWrapperService>())
        {
        }
    }
}
"));

            await test.RunAsync();
        }
    }
}
