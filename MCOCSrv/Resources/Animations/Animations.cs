using CommunityToolkit.Maui.Extensions;

namespace MCOCSrv.Resources.Animations
{
    public static class Animations
    {
        public static void AttachHoverButtonAnimation(object? obj)
        {
            //Check if the object is a view and if it already doesnt have a Recognizer attached
            if (obj is View v && !v.GestureRecognizers.OfType<PointerGestureRecognizer>().Any())
            {
                var recognizer = new PointerGestureRecognizer();
                recognizer.PointerEntered += HoverButtonAnimation;
                recognizer.PointerExited += HoverButtonAnimationRev;
                v.GestureRecognizers.Add(recognizer);
            }
        }

        public static void AttachHoverAnimationConsoleActions(object? obj)
        {
            if (obj is View v && !v.GestureRecognizers.OfType<PointerGestureRecognizer>().Any() && v is HorizontalStackLayout layout)
            {
                foreach (var item in layout.Children.OfType<View>())
                {
                    if (item is Label label)
                    {
                        var recognizer = new PointerGestureRecognizer();
                        label.IsVisible = false;
                        label.IsEnabled = false;
                        recognizer.PointerEntered += HoverConsoleActionAnimation;
                        recognizer.PointerExited += HoverConsoleActionAnimationRev;
                        layout.GestureRecognizers.Add(recognizer);
                    }
                }
            }
        }

        public static async void HoverButtonAnimation(object? o, PointerEventArgs e)
        {
            if (o is View v)
            {
                Task scale = v.ScaleTo(1.03, 150, Easing.Linear);
                Func<Task> bgcolor = () =>
                {
                    if (v is Button b && b.BorderColor != Colors.Transparent)
                        return b.BackgroundColorTo(b.BorderColor, 16, 150);
                    if (v is Border border && border.Stroke != Brush.Transparent)
                        return border.BackgroundColorTo((border.Stroke is SolidColorBrush brush) ? brush.Color : Colors.Transparent, 16, 150);
                    return Task.CompletedTask;
                };
                await Task.WhenAny(scale, bgcolor());
            }
        }

        public static async void HoverButtonAnimationRev(object? o, PointerEventArgs e)
        {
            if (o is View v)
            {
                Task scale = v.ScaleTo(1, 150, Easing.Linear);
                Func<Task> bgcolor = () =>
                {
                    if (v is Button b && b.BorderColor != Colors.Transparent)
                        return b.BackgroundColorTo(Colors.Transparent, 16, 150);
                    if (v is Border border)
                        return border.BackgroundColorTo(Colors.Transparent, 16, 150);
                    return Task.CompletedTask;
                };
                await Task.WhenAny(scale, bgcolor());
            }
        }

        public static async void HoverConsoleActionAnimation(object? o, PointerEventArgs e)
        {
            if (o is HorizontalStackLayout layout)
            {
                foreach (var child in layout.Children.OfType<Label>())
                {
                    child.IsVisible = true;
                    child.IsEnabled = true;
                    Task task = child.FadeTo(1, 200, Easing.Linear);
                    await task;
                }
                foreach (var child in layout.Children.OfType<Image>())
                {
                    //Task task = child.TranslateTo(child.TranslationX + child.TranslationX, child.TranslationY, 200, Easing.Linear);
                    //await task;
                }
            }
        }

        public static async void HoverConsoleActionAnimationRev(object? o, PointerEventArgs e)
        {
            if (o is HorizontalStackLayout layout)
            {
                foreach (var child in layout.Children.OfType<Label>())
                {
                    Task task = child.FadeTo(0, 200, Easing.Linear);
                    await task;
                    child.IsVisible = false;
                    child.IsEnabled = false;
                }
                foreach (var child in layout.Children.OfType<Image>())
                {
                    //Task task = child.TranslateTo(child.TranslationX - 10, child.TranslationY, 200, Easing.Linear);
                    //await task;
                }
            }
        }

        public static async void FadeInAnimation(object? o)
        {
            if (o is View v)
                await v.FadeTo(1, 200, Easing.Linear);
        }

        public static async Task FadeOutAnimation(object? o)
        {
            if (o is View v)
                await v.FadeTo(0, 200, Easing.Linear);
        }
    }
}
