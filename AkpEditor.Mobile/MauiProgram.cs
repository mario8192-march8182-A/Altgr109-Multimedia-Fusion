using AkpEditor.Mobile.Services;
using AkpEditor.Mobile.Services.Export;

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
        builder.Services.AddSingleton<ExportPage>();

        return builder;
    }

    private static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<ViewModels.EditorViewModel>();
        builder.Services.AddSingleton<ViewModels.AssetManagerViewModel>();
        builder.Services.AddSingleton<ViewModels.ProjectSettingsViewModel>();
        builder.Services.AddSingleton<ViewModels.ExportViewModel>();

        return builder;
    }

    private static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<EditorService>();
        builder.Services.AddSingleton<ProjectService>();
        builder.Services.AddSingleton<AssetDatabaseService>();
        builder.Services.AddSingleton<AssetPreviewService>();
        builder.Services.AddSingleton<AssetValidationService>();
        builder.Services.AddSingleton<ExportManagerService>();

        return builder;
    }
}