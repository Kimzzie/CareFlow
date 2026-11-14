using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CareFlow.Application.Abstractions;
using CareFlow.Application.Services;
using CareFlow.Desktop.ViewModels;
using CareFlow.Desktop.Views;
using CareFlow.Infrastructure;
using CareFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareFlow.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddInfrastructure(CareFlowDatabase.DefaultConnectionString);
        services.AddTransient<IBookingService, BookingService>();
        services.AddTransient<BookingFormViewModel>();
        services.AddTransient<BookingHistoryViewModel>();
        services.AddTransient<MainWindowViewModel>();
        var provider = services.BuildServiceProvider();

        // Reproducible setup: a fresh machine builds its own database from the migrations
        using (var scope = provider.CreateScope())
            scope.ServiceProvider.GetRequiredService<CareFlowDbContext>().Database.Migrate();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = provider.GetRequiredService<MainWindowViewModel>();
            var window = new MainWindow { DataContext = vm };
            window.Opened += async (_, _) => await vm.InitializeAsync();   // event handler: async void is OK here
            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}