namespace WheelOfNames;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        BindingContext = new MainPageViewModel();
    }

    private async void Wheel_OnNameSelected(string name)
    {
        await DisplayAlert("Today's post is by", name, "OK");
    }
}