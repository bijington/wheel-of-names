using System.Collections;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Windows.Input;
using CommunityToolkit.Maui.Core.Extensions;

namespace WheelOfNames;

public class WheelView : GraphicsView, IDrawable
{
    private const string AnimationName = "SpinWheel";
    
    private readonly IList<Color> colors = [Colors.Chartreuse, Colors.Blue, Colors.Green, Colors.Red, Colors.DeepPink];
    private float angle;
    private float rotation;
    private IList<string> names = [];

    public event Action<string>? NameSelected;
    
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(WheelView), null, propertyChanged: OnItemsSourcePropertyChanged);
    
    private static void OnItemsSourcePropertyChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        if (bindable is WheelView wheelView)
        {
            wheelView.HandleItemsSourcePropertyChanged(oldValue, newValue);
        }
    }

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    
    public static readonly BindableProperty NameSelectedCommandProperty =
        BindableProperty.Create(nameof(NameSelectedCommand), typeof(ICommand), typeof(WheelView), null);

    public ICommand? NameSelectedCommand
    {
        get => (ICommand)GetValue(NameSelectedCommandProperty);
        set => SetValue(NameSelectedCommandProperty, value);
    }
    
    private void HandleItemsSourcePropertyChanged(object? oldValue, object? newValue)
    {
        if (oldValue is INotifyCollectionChanged oldNotifyCollectionChanged)
        {
            oldNotifyCollectionChanged.CollectionChanged -= NotifyCollectionChangedOnCollectionChanged;
        }
        
        if (newValue is IEnumerable<string> stringEnumerable)
        {
            UpdateNames(stringEnumerable.ToList());
        }

        if (newValue is INotifyCollectionChanged notifyCollectionChanged)
        {
            notifyCollectionChanged.CollectionChanged += NotifyCollectionChangedOnCollectionChanged;
        }
    }

    private void NotifyCollectionChangedOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems is not null &&
                    e.NewItems.Count > 0 &&
                    e.NewItems[0] is string newName)
                {
                    names.Insert(e.NewStartingIndex, newName);   
                }
                break;
            
            case NotifyCollectionChangedAction.Remove:
                names.RemoveAt(e.OldStartingIndex);   
                break;
            
            case NotifyCollectionChangedAction.Replace:
                break;
            
            case NotifyCollectionChangedAction.Move:
                break;
            
            case NotifyCollectionChangedAction.Reset:
                break;
            
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        this.UpdateNames(names);
    }

    public static readonly BindableProperty SpinCommandProperty =
        BindableProperty.CreateReadOnly(nameof(SpinCommand), typeof(ICommand), typeof(WheelView), default, BindingMode.OneWayToSource, defaultValueCreator: CreateSpinCommand).BindableProperty;
    
    public ICommand SpinCommand
    {
        get => (ICommand)GetValue(SpinCommandProperty);
    }
    
    static Command CreateSpinCommand(BindableObject bindable)
    {
        var wheelView = (WheelView)bindable;
        return new Command(() => wheelView.Spin(), () => wheelView.AnimationIsRunning(AnimationName) is false);
    }

    public WheelView()
    {
        Drawable = this;
    }

    private void UpdateNames(IList<string> newNames)
    {
        this.names = newNames;
        angle = 360f / names.Count;
        
        this.Invalidate();
    }
    
    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        int colorIndex = 0;
        float padding = 10f;
        var radius = Math.Min(dirtyRect.Height / 2, dirtyRect.Width / 2) - padding * 2;

        float startAngle = rotation;

        PointF center = dirtyRect.Center;

        RectF rect = new RectF(
            center.X - radius,
            center.Y - radius,
            radius * 2,
            radius * 2);

        foreach (var name in names)
        {
            float sweepAngle = angle;

            var path = new PathF();
            path.MoveTo(center);
            path.AddArc(rect.Left, rect.Top, rect.Right, rect.Bottom, startAngle, startAngle - sweepAngle, true);
            path.Close();
            
            canvas.FillColor = colors[colorIndex];
            canvas.StrokeSize = 2;
            canvas.StrokeColor = Colors.White;
            
            canvas.DrawPath(path);
            canvas.FillPath(path);

            canvas.FontColor = colors[colorIndex].ToBlackOrWhiteForText();
            canvas.FontSize = 30;
            
            canvas.SaveState();
            canvas.Translate(center.X, center.Y);
            canvas.Rotate(360 - (startAngle - (angle / 2)));
            canvas.DrawString(name, 0, -radius / 4, radius - padding, radius / 2, HorizontalAlignment.Right, VerticalAlignment.Center);

            canvas.RestoreState();

            colorIndex = (colorIndex + 1) >= colors.Count ? 0 : colorIndex + 1;

            startAngle += sweepAngle;
        }
        
        canvas.FillColor = Colors.White;
        var innerCircleRadius = radius / 4;
        canvas.FillEllipse(
            dirtyRect.Center.X - innerCircleRadius / 2,
            dirtyRect.Center.Y - innerCircleRadius / 2,
            innerCircleRadius,
            innerCircleRadius);
    }
    
    private void Spin()
    {
        Animation animation = new Animation();

        var finalAngle = RandomNumberGenerator.GetInt32(0, 360);
        var numberOfSpins = RandomNumberGenerator.GetInt32(3, 7);

        animation.Add(
            0.0,
            1.0,
            new Animation(
                v =>
                {
                    rotation = (float)v;

                    this.Invalidate();
                },
                rotation,
                finalAngle + (360 * numberOfSpins)));

        animation.Commit(
            this,
            AnimationName,
            16,
            10_000,
            Easing.CubicInOut,
            finished: (value, finished) =>
            {
                var winningIndex = (int)(finalAngle / angle);

                var winningName = names[winningIndex];
                
                this.NameSelected?.Invoke(names[winningIndex]);
                
                if (NameSelectedCommand is not null && NameSelectedCommand.CanExecute(winningName))
                {
                    NameSelectedCommand.Execute(winningName);   
                }
            });
    }
}