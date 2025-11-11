using System;
using System.Windows;

namespace Armonia.App
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
        {
            MessageBox.Show($"Unhandled Exception: {args.ExceptionObject}");
        };

        DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show($"Dispatcher Exception: {args.Exception.Message}\n\n{args.Exception.StackTrace}");
            args.Handled = true;
        };

        base.OnStartup(e);

            var mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}

