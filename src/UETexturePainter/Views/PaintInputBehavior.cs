using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;
using UETexturePainter.ViewModels;

namespace UETexturePainter.Views;

public static class PaintInputBehavior
{
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(PaintInputBehavior),
            new PropertyMetadata(false, OnIsEnabledChanged));

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.RegisterAttached("ViewModel", typeof(MainViewModel), typeof(PaintInputBehavior));

    public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

    public static MainViewModel? GetViewModel(DependencyObject obj) => (MainViewModel?)obj.GetValue(ViewModelProperty);
    public static void SetViewModel(DependencyObject obj, MainViewModel? value) => obj.SetValue(ViewModelProperty, value);

    private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Shape shape)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            shape.MouseDown += OnMouseDown;
            shape.MouseMove += OnMouseMove;
            shape.MouseUp += OnMouseUp;
        }
        else
        {
            shape.MouseDown -= OnMouseDown;
            shape.MouseMove -= OnMouseMove;
            shape.MouseUp -= OnMouseUp;
        }
    }

    private static void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Shape shape)
        {
            return;
        }

        var viewModel = GetViewModel(shape);
        if (viewModel is null)
        {
            return;
        }

        viewModel.UpdateCanvasSize(shape.ActualWidth, shape.ActualHeight);
        shape.CaptureMouse();
        var position = e.GetPosition(shape);
        viewModel.BeginStroke(position);
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not Shape shape)
        {
            return;
        }

        var viewModel = GetViewModel(shape);
        if (viewModel is null || e.LeftButton != MouseButtonState.Pressed)
        {
            return;
        }

        viewModel.UpdateCanvasSize(shape.ActualWidth, shape.ActualHeight);
        var position = e.GetPosition(shape);
        viewModel.ContinueStroke(position);
    }

    private static void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Shape shape)
        {
            return;
        }

        var viewModel = GetViewModel(shape);
        if (viewModel is null)
        {
            return;
        }

        viewModel.UpdateCanvasSize(shape.ActualWidth, shape.ActualHeight);
        shape.ReleaseMouseCapture();
        var position = e.GetPosition(shape);
        viewModel.EndStroke(position);
    }
}
