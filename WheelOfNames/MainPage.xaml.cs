using System.Diagnostics;
using System.Security.Cryptography;
using Color = Microsoft.Maui.Graphics.Color;
using RectF = Microsoft.Maui.Graphics.RectF;

namespace WheelOfNames;

public partial class MainPage : ContentPage
{
    private readonly Wheel wheel;

    public MainPage()
    {
        InitializeComponent();
        wheel = new Wheel(this);
        WheelView.Drawable = wheel;

        wheel.Finished += WheelOnFinished;
    }

    private async void WheelOnFinished(string obj)
    {
        await DisplayAlert("And the winner is", obj, "OK");
    }

    private void Wheel_OnStartInteraction(object? sender, TouchEventArgs e)
    {
        wheel.Spin();
    }

    internal void Invalidate()
    {
        this.WheelView.Invalidate();
    }

    private void NamesEditor_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        this.wheel.UpdateNames(e.NewTextValue.Split(
            [Environment.NewLine],
            StringSplitOptions.RemoveEmptyEntries
        ));
        
        this.Invalidate();
    }
}