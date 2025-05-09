using System.Diagnostics;
using System.Security.Cryptography;

namespace WheelOfNames;

public class Wheel : IDrawable
{
    private readonly MainPage mainPage;
    private readonly IList<Color> colors = [Colors.Chartreuse, Colors.Blue, Colors.Green, Colors.Red, Colors.DeepPink];

    private float angle;
    private float rotation;

    public IList<string> Names { get; private set; }

    public void UpdateNames(string[] names)
    {
        this.Names = names;
        angle = 360f / Names.Count;
    }

    public Wheel(MainPage mainPage)
    {
        this.mainPage = mainPage;
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

        foreach (var name in Names)
        {
            float sweepAngle = angle;

            var path = new PathF();
            path.MoveTo(center);
            path.AddArc(rect.Left, rect.Top, rect.Right, rect.Bottom, startAngle, startAngle + sweepAngle, false);
            path.Close();

            canvas.SaveState();
            canvas.FillColor = colors[colorIndex];
            canvas.StrokeSize = 2;
            canvas.StrokeColor = Colors.White;

            canvas.DrawPath(path);
            canvas.FillPath(path);

            canvas.FontColor = Colors.White;
            canvas.FontSize = 50;
            canvas.Translate(center.X, center.Y);
            canvas.Rotate(360 - (startAngle - (angle / 2)));
            canvas.DrawString(name, 0, -radius / 4, radius - padding, radius / 2, HorizontalAlignment.Right, VerticalAlignment.Center);
            
            Debug.WriteLine($"{name} - {startAngle} and {startAngle + sweepAngle} text {360 - (startAngle - (angle / 2))}");

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

    public event Action<string> Finished;

    internal void Spin()
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

                    this.mainPage.Invalidate();
                },
                0,
                finalAngle + (360 * numberOfSpins)));

        animation.Commit(
            this.mainPage,
            "Wheel",
            16,
            10_000,
            Easing.CubicInOut,
            finished: (value, finished) =>
            {
                var winningIndex = (int)(finalAngle / angle);
                
                Debug.WriteLine($"{finalAngle} - {winningIndex}");

                this.Finished?.Invoke(Names[winningIndex]);
            });
    }
}