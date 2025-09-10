using Microsoft.Maui.Controls;

namespace PhotoUploader;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        // Dodaj NavigationPage jako główną stronę
        var navigationPage = new NavigationPage(new MainPage())
        {
            BarBackgroundColor = Color.FromArgb("#2196F3"),
            BarTextColor = Colors.White
        };

        return new Window(navigationPage)
        {
            Title = "Photo Uploader"
        };
    }
}