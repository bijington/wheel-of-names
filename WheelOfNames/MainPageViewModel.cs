using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace WheelOfNames;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string? nameText = string.Empty;

    [ObservableProperty] 
    private ObservableCollection<string> names = [];

    partial void OnNameTextChanged(string? value)
    {
        if (value is null)
        {
            Names = [];
            return;
        }
        
        Names = new ObservableCollection<string>(value.Split(
            [Environment.NewLine],
            StringSplitOptions.RemoveEmptyEntries
        ));
    }

    [RelayCommand]
    private void NameSelected(string name)
    {
        this.Names.Remove(name);
    }
}