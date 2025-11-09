using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KlabTranslations.Core;

namespace KlabTranslations.AvaloniaExample.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _userName = "World";

    [ObservableProperty]
    private int _messageCount = 0;

    public static string CurrentCulture => TranslationProvider.CurrentCulture.Name;

    public string Greeting { get; } = "Welcome to Avalonia!";

    [RelayCommand]
    public void SetCulture(object parameter)
    {
        switch (parameter)
        {
            case "en":
                TranslationProvider.SetCulture(CultureInfo.GetCultureInfo("en-US"));
                break;

            case "de":
                TranslationProvider.SetCulture(CultureInfo.GetCultureInfo("de"));
                break;

            case "de-AT":
                TranslationProvider.SetCulture(CultureInfo.GetCultureInfo("de-AT"));
                break;

            case "zh":
                TranslationProvider.SetCulture(CultureInfo.GetCultureInfo("zh-CN"));
                break;
        }

        OnPropertyChanged(nameof(CurrentCulture));
    }
}
