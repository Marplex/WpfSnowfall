using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using WpfSnowfall.Snowflakes;
using WpfSnowfall.Models;

namespace WpfSnowfall;

public class Snowfall : Canvas
{

    private readonly Random _random = new();
    private DispatcherTimer? _timer;

    /// <summary>
    /// Property for <see cref="ScaleFactor"/>.
    /// </summary>
    public static readonly DependencyProperty ScaleFactorProperty = DependencyProperty.Register(
        nameof(ScaleFactor), typeof(double), typeof(Snowfall), new(0.5));

    /// <summary>
    /// Property for <see cref="EmissionRate"/>.
    /// </summary>
    public static readonly DependencyProperty EmissionRateProperty = DependencyProperty.Register(
        nameof(EmissionRate), typeof(int), typeof(Snowfall), new(5));

    /// <summary>
    /// Property for <see cref="OpacityFactor"/>.
    /// </summary>
    public static readonly DependencyProperty OpacityFactorProperty = DependencyProperty.Register(
        nameof(OpacityFactor), typeof(double), typeof(Snowfall), new(3.0));

    /// <summary>
    /// Property for <see cref="ParticleSpeed"/>.
    /// </summary>
    public static readonly DependencyProperty ParticleSpeedProperty = DependencyProperty.Register(
        nameof(ParticleSpeed), typeof(double), typeof(Snowfall), new(1.0));

    /// <summary>
    /// Property for <see cref="Fill"/>.
    /// </summary>
    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
        nameof(Fill), typeof(Brush), typeof(Snowfall), new(new SolidColorBrush(Color.FromRgb(255, 255, 255))));

    /// <summary>
    /// Property for <see cref="LeaveAnimation"/>.
    /// </summary>
    public static readonly DependencyProperty LeaveAnimationProperty = DependencyProperty.Register(
        nameof(LeaveAnimation), typeof(SnowflakeAnimation), typeof(Snowfall), new(SnowflakeAnimation.None));
    

    /// <summary>
    /// Scale in/out snowflakes. Higher values generates bigger snowflakes.
    /// Default: 1.0
    /// </summary>
    public double ScaleFactor
    {
        get => (double)GetValue(ScaleFactorProperty);
        set => SetValue(ScaleFactorProperty, value);
    }

    /// <summary>
    /// How many snowflakes to emit every second.
    /// Default: 5
    /// </summary>
    public int EmissionRate
    {
        get => (int)GetValue(EmissionRateProperty);
        set => SetValue(EmissionRateProperty, value);
    }

    /// <summary>
    /// Snowflake opacity.
    /// Default: 1.0
    /// </summary>
    public double OpacityFactor
    {
        get => (double)GetValue(OpacityFactorProperty);
        set => SetValue(OpacityFactorProperty, value);
    }

    /// <summary>
    /// Snowflake fall speed
    /// </summary>
    public double ParticleSpeed
    {
        get => (double)GetValue(ParticleSpeedProperty);
        set => SetValue(ParticleSpeedProperty, value);
    }

    /// <summary>
    /// Snowflake leave animation
    /// </summary>
    public SnowflakeAnimation LeaveAnimation
    {
        get => (SnowflakeAnimation)GetValue(LeaveAnimationProperty);
        set => SetValue(LeaveAnimationProperty, value);
    }

    /// <summary>
    /// Snowflake color
    /// </summary>
    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public Snowfall()
    {
        Loaded += Snowfall_Loaded;
        Unloaded += Snowfall_Unloaded;
    }

    private void Snowfall_Unloaded(object sender, RoutedEventArgs e)
    {
        _timer?.Stop();
    }

    private void Snowfall_Loaded(object sender, RoutedEventArgs e)
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds((int)(1000.0 / EmissionRate)) };
        _timer.Tick += (s, arg) => EmitSnowflake();
        _timer.Start();
    }

    private void EmitSnowflake()
    {
        //Initial snowflake state
        var xAmount = _random.Next(0, (int)ActualWidth);
        var scale = ((_random.NextDouble() * 0.6) + 0.5) * ScaleFactor;
        var rotateAmount = _random.Next(0, 270);

        RotateTransform rotateTransform = new(rotateAmount);
        ScaleTransform scaleTransform = new(scale, scale);
        TranslateTransform translateTransform = new(xAmount, -(50 * ScaleFactor));

        //Setup animation time
        var duration = new Duration(TimeSpan.FromSeconds(_random.Next(8, 10) * (1.0 / ParticleSpeed)));
        var fadeDuration = new Duration(TimeSpan.FromSeconds(2));

        //Create snowflake
        var flake = Snowflake.Generate();
        flake.Foreground = Fill;
        flake.RenderTransformOrigin = new(0.5, 0.5);
        flake.Opacity = ((_random.NextDouble() * 0.5) + 0.5) * OpacityFactor;
        flake.HorizontalAlignment = HorizontalAlignment.Left;
        flake.VerticalAlignment = VerticalAlignment.Top;
        flake.RenderTransform = new TransformGroup
        {
            Children = new TransformCollection { rotateTransform, scaleTransform, translateTransform }
        };

        Children.Add(flake);

        //Create transform animations
        xAmount += _random.Next(-100, 100);
        var xAnimation = GenerateAnimation(xAmount, duration, flake, "RenderTransform.Children[2].X");

        var yAmount = (int)(ActualHeight + (50 * ScaleFactor));
        var yAnimation = GenerateAnimation(yAmount, duration, flake, "RenderTransform.Children[2].Y");

        rotateAmount += _random.Next(90, 360);
        var rotateAnimation = GenerateAnimation(rotateAmount, duration, flake, "RenderTransform.Children[0].Angle");

        //Create fade animations
        var fadeOutAnimation = GenerateAnimation(.0, fadeDuration, flake, "Opacity", duration.Subtract(fadeDuration).TimeSpan);


        //Start animation
        Storyboard story = new();
        story.Children.Add(xAnimation);
        story.Children.Add(yAnimation);
        story.Children.Add(rotateAnimation);
        
        if (LeaveAnimation == SnowflakeAnimation.Fade)
            story.Children.Add(fadeOutAnimation);

        flake.Loaded += (sender, args) => story.Begin();
        story.Completed += (sender, e) => Children.Remove(flake);

    }

    private static DoubleAnimation GenerateAnimation(double x, Duration duration, UserControl flake, string propertyPath, TimeSpan? beginTime = null)
    {
        DoubleAnimation animation = new()
        {
            BeginTime = beginTime ?? TimeSpan.Zero,
            To = x,
            Duration = duration
        };

        Storyboard.SetTarget(animation, flake);
        Storyboard.SetTargetProperty(animation, new PropertyPath(propertyPath));
        return animation;
    }
}