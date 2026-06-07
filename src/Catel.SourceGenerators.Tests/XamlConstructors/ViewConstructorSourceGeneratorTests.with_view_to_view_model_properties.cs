namespace Catel.SourceGenerators.Tests.XamlConstructors;

using Catel.SourceGenerators.XamlConstructors;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using VerifyNUnit;

[TestFixture]
public partial class ViewCtorGeneratorTests
{
    [Test]
    public async Task Generates_ViewToViewModel_Property_Subscriptions()
    {
        var driver = BuildViewWithViewToViewModelPropertiesDriver();

        await Verifier.Verify(driver)
            .ScrubAssemblyVersion();
    }

    private GeneratorDriver BuildViewWithViewToViewModelPropertiesDriver()
    {
        var userControlSource = @"
using System.Windows;
using System.Windows.Controls;
using Catel.IoC;
using Catel.MVVM;
using Catel.MVVM.Views;

namespace MyNamespace
{
    public partial class MyUserControl : Catel.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(MyUserControl));

        [ViewToViewModel]
        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register(nameof(Count), typeof(int), typeof(MyUserControl));

        [ViewToViewModel(MappingType = ViewToViewModelMappingType.ViewToViewModel)]
        public int Count
        {
            get => (int)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }

        public string NotMapped { get; set; }
    }

    public partial class MyUserControl
    {
        public void InitializeComponent()
        {
        }
    }

    public interface ILogger<T> {}
    public interface IUserControlWrapperService {}
}
";

        var iocContainerSource = @"
namespace System.Windows
{
    public sealed class DependencyProperty
    {
        public static DependencyProperty Register(string name, System.Type propertyType, System.Type ownerType) => new DependencyProperty();
    }

    public class DependencyObject
    {
        protected object GetValue(DependencyProperty dp) => throw new System.NotImplementedException();
        protected void SetValue(DependencyProperty dp, object value) {}
    }
}

namespace Catel.MVVM
{
    public interface IViewModel
    {
    }
}

namespace Catel.MVVM.Views
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class ViewToViewModelAttribute : System.Attribute
    {
        public ViewToViewModelAttribute(string viewModelPropertyName = "")
        {
            MappingType = ViewToViewModelMappingType.TwoWayViewModelWins;

            ViewModelPropertyName = viewModelPropertyName;
        }

        /// <summary>
        /// Gets or sets the view model property name.
        /// </summary>
        /// <value>The view model property name.</value>
        public string ViewModelPropertyName { get; private set; }

        /// <summary>
        /// Gets or sets the type of the mapping.
        /// </summary>
        /// <value>The type of the mapping.</value>
        public ViewToViewModelMappingType MappingType { get; set; }
    }

    public enum ViewToViewModelMappingType
    {
        TwoWayDoNothing,

        TwoWayViewWins,

        TwoWayViewModelWins,

        ViewToViewModel,

        ViewModelToView
    }
}

namespace Catel.Windows.Controls
{
    public class UserControl : System.Windows.Controls.UserControl
    {
        protected UserControl(ILogger logger, IUserControlWrapperService service) {}
    }

    public interface ILogger {}
    public interface IUserControlWrapperService {}
}

namespace Catel.IoC
{
    public static class IoCContainer
    {
        public static ProviderType ServiceProvider { get; } = new ProviderType();
        public class ProviderType
        {
            public T GetRequiredService<T>() => default(T);
        }
    }
}
";

        var compilation = CSharpCompilation.Create("name", new[]
        {
            userControlSource,
            iocContainerSource
        }.Select(x => CSharpSyntaxTree.ParseText(x)));

        var generator = new ViewConstructorsSourceGenerator();

        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }
}
