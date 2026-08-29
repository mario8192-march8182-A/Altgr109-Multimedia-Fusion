using AkpEditor.Mobile.Views;

namespace AkpEditor.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .RegisterPages()
            .RegisterViewModels()
            .RegisterServices();

        return builder.Build();
    }

    private static MauiAppBuilder RegisterPages(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<EditorPage>();
        builder.Services.AddSingleton<AssetManagerPage>();
        builder.Services.AddSingleton<ProjectSettingsPage>();

        return builder;
    }

    private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<ViewModels.EditorViewModel>();
        builder.Services.AddSingleton<ViewModels.AssetManagerViewModel>();
        builder.Services.AddSingleton<ViewModels.ProjectSettingsViewModel>();

        return builder;
    }

    private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<Services.EditorService>();
        builder.Services.AddSingleton<Services.ProjectService>();
        builder.Services.AddSingleton<Services.AssetService>();

        return builder;
    }
}