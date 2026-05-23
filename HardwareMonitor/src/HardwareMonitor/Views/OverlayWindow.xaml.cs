using System.Windows;
using System.Windows.Input;

namespace HardwareMonitor.Views;

public partial class OverlayWindow : Window
{
    public OverlayWindow()
    {
        InitializeComponent();
        MouseLeftButtonDown += (_, e) =>
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        };
    }

    private void CloseClick(object sender, MouseButtonEventArgs e)
    {
        Hide();
    }
}
