using System.Windows.Input;

namespace MCOCSrv.Resources.Content;

public partial class ConsoleActionButton : ContentView
{
    public static BindableProperty CommandProperty = BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(ConsoleActionButton), default(ICommand), propertyChanged: OnCommandChanged);
    public ICommand Command
    {
        get => (ICommand)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static BindableProperty GlyphProperty = BindableProperty.Create(nameof(Glyph), typeof(string), typeof(ConsoleActionButton), default(string), propertyChanged: OnGlyphChanged);
    public string Glyph
    {
        get => (string)GetValue(GlyphProperty);
        set => SetValue(GlyphProperty, value);
    }

    public static BindableProperty ColorProperty = BindableProperty.Create(nameof(GlyphColor), typeof(Color), typeof(ConsoleActionButton), default(Color), propertyChanged: OnColorChanged);
    public Color GlyphColor
    {
        get => (Color)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }
    public static BindableProperty ActionNameProperty = BindableProperty.Create(nameof(ActionName), typeof(string), typeof(ConsoleActionButton), default(string), propertyChanged: OnNameChanged);
    public string ActionName
    {
        get => (string)GetValue(ActionNameProperty);
        set => SetValue(ActionNameProperty, value);
    }
    public ConsoleActionButton()
    {
        InitializeComponent();
        AssignAnimation();
    }

    private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ConsoleActionButton button && newValue is ICommand command)
        {
            button.ButtonBodyRecognizer.Command = command;
        }
    }

    private static void OnGlyphChanged(BindableObject bindable, object value, object newValue)
    {
        if (bindable is ConsoleActionButton button && newValue is string glyph)
        {
            button.ActionButtonImage.Source = new FontImageSource
            {
                FontFamily = "MDI",
                Glyph = glyph,
                Color = button.GlyphColor,
            };
        }
    }

    private static void OnColorChanged(BindableObject bindable, object value, object newValue)
    {
        if (bindable is ConsoleActionButton button && newValue is Color color)
        {
            if (button.ActionButtonImage.Source is FontImageSource fontImage)
            {
                fontImage.Color = color;
                button.ActionButtonImage.Source = fontImage;
            }
        }
    }

    private static void OnNameChanged(BindableObject bindable, object value, object newValue)
    {
        if (bindable is ConsoleActionButton button && newValue is string name)
        {
            button.ActionButtonText.Text = name;
        }
    }
    private void AssignAnimation()
    {
        Animations.Animations.AttachHoverAnimationConsoleActions(ButtonBody);
    }
}