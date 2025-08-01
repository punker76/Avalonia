using System;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace IntegrationTestApp
{
    public class MeasureBorder : Border
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            MeasuredWith = availableSize;
            
            return base.MeasureOverride(availableSize);
        }

        public static readonly StyledProperty<Size> MeasuredWithProperty = AvaloniaProperty.Register<MeasureBorder, Size>(
            nameof(MeasuredWith));

        public Size MeasuredWith
        {
            get => GetValue(MeasuredWithProperty);
            set => SetValue(MeasuredWithProperty, value);
        }
    }
    
    public partial class ShowWindowTest : Window
    {
        private readonly DispatcherTimer? _timer;
        private readonly TextBox? _orderTextBox;
        private int _mouseMoveCount;
        private int _mouseReleaseCount;

        public ShowWindowTest()
        {
            InitializeComponent();
            DataContext = this;
            PositionChanged += (s, e) => CurrentPosition.Text = $"{Position}";

            PointerMoved += OnPointerMoved;
            PointerReleased += OnPointerReleased;
            PointerExited += (_, e) => ResetCounters();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _orderTextBox = CurrentOrder;
                _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
                _timer.Tick += TimerOnTick;
                _timer.Start();
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            var scaling = PlatformImpl!.DesktopScaling;
            CurrentPosition.Text = $"{Position}";
            CurrentScreenRect.Text = $"{Screens.ScreenFromVisual(this)?.WorkingArea}";
            CurrentScaling.Text = $"{scaling}";

            if (Owner is not null)
            {
                var owner = (Window)Owner;
                CurrentOwnerRect.Text = $"{owner.Position}, {PixelSize.FromSize(owner.FrameSize!.Value, scaling)}";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _timer?.Stop();
        }

        private void TimerOnTick(object? sender, EventArgs e)
        {
            _orderTextBox!.Text = MacOSIntegration.GetOrderedIndex(this).ToString();
        }

        private void AddToWidth_Click(object? sender, RoutedEventArgs e) => Width = Bounds.Width + 10;
        private void AddToHeight_Click(object? sender, RoutedEventArgs e) => Height = Bounds.Height + 10;
        
        private void OnPointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            _mouseMoveCount++;
            UpdateCounterDisplays();
        }
        
        private void OnPointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
        {
            _mouseReleaseCount++;
            UpdateCounterDisplays();
        }
        
        public void ResetCounters()
        {
            _mouseMoveCount = 0;
            _mouseReleaseCount = 0;
            UpdateCounterDisplays();
        }
        
        private void UpdateCounterDisplays()
        {
            var mouseMoveCountTextBox = this.FindControl<TextBox>("MouseMoveCount");
            var mouseReleaseCountTextBox = this.FindControl<TextBox>("MouseReleaseCount");
            
            if (mouseMoveCountTextBox != null)
                mouseMoveCountTextBox.Text = _mouseMoveCount.ToString();
            
            if (mouseReleaseCountTextBox != null)
                mouseReleaseCountTextBox.Text = _mouseReleaseCount.ToString();
        }
        
        public void ShowTitleAreaControl()
        {
            var titleAreaControl = this.FindControl<Grid>("TitleAreaControl");
            if (titleAreaControl == null) return;
            titleAreaControl.IsVisible = true;
                
            var titleBarHeight = ExtendClientAreaTitleBarHeightHint > 0 ? ExtendClientAreaTitleBarHeightHint : 30;
            titleAreaControl.Margin = new Thickness(110, -titleBarHeight, 8, 0);
            titleAreaControl.Height = titleBarHeight;
        }
    }
}
