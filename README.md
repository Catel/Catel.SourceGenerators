# Catel.SourceGenerators

Experimental source generators for Catel.

## Note for debugging

If the source generator is generating errors, add this to the csproj:

```

  <PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
    <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
  </PropertyGroup>

  <ItemGroup>
    <Compile Remove="$(CompilerGeneratedFilesOutputPath)/**/*.cs" />
  </ItemGroup>
  
```

Run the build one more time, then disable the source generator and comment the compile remove line. Then the code can be checked inside Visual Studio.

## Xaml Constructors

Xaml requires empty constructors, so they must be provided. But as developers, we want dependency injection (DI). The DI is "required" for the `IViewModelLocator`, etc.

The developers can write the constructors they want (with dependency injection). Then the source generator will handle the generation 
of the empty constructors.

Code written by developer:

```
public MyUserControl(ILogger<MyUserControl> logger, IUserControlWrapperService userControlWrapperService)
  : base(logger, userControlWrapperService)
{
    InitializeComponent();
}
```

Final result:

```
[CompilerGenerated]
public MyUserControl()
  : this(IoCContainer.ServiceProvider.GetRequiredService<ILogger<MyUserControl>>(), IoCContainer.ServiceProvider.GetRequiredService<IUserControlWrapperService>())
{
}


public MyUserControl(ILogger<MyUserControl> logger, IUserControlWrapperService userControlWrapperService)
  : base(logger, userControlWrapperService)
{
    InitializeComponent();
}
```