namespace Catel.SourceGenerators.Tests;

using Catel.SourceGenerators.XamlConstructors;
using VerifyTests;

public static class VerifySettingsTaskExtensions
{
    private static readonly string AssemblyVersion = typeof(BehaviorConstructorInfo).Assembly.GetName().Version!.ToString();

    public static SettingsTask ScrubAssemblyVersion(this SettingsTask settings)
    {
        return settings.ScrubLinesWithReplace(x =>
        {
            x = x.Replace(AssemblyVersion, "{AssemblyVersion}");

            return x;
        });
    }
}
