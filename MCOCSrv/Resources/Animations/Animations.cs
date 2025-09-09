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
            if (obj is View v && !v.GestureRecognizers.OfType<PointerGestureRecognizer>().Any() && v is Layout layout)
            {
                foreach (var item in layout.Children.OfType<View>())
                {
                    if (item is Label label)
                    {
                        var recognizer = new PointerGestureRecognizer();
                        recognizer.PointerEntered += ConsoleButtonAnimation;
                        recognizer.PointerExited += ConsoleButtonAnimationRev;
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

        public static async void ConsoleButtonAnimation(object? o, PointerEventArgs e)
        {
            if (o is AbsoluteLayout layout)
            {
                var Lwidth = layout.Width;
                foreach (var child in layout.Children.OfType<VisualElement>())
                {
                    Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(child);
                }
                foreach (var child in layout.Children.OfType<Image>())
                {
                    await child.TranslateTo((child.Width - Lwidth) / 2 + child.Width / 2, 0, 200, Easing.SinIn);
                }
                foreach (var child in layout.Children.OfType<Label>())
                {
                    await child.FadeTo(1, 200, Easing.SinIn);
                }
            }
        }

        public static async void ConsoleButtonAnimationRev(object? o, PointerEventArgs e)
        {
            if (o is AbsoluteLayout layout)
            {
                var Lwidth = layout.Width;
                foreach (var child in layout.Children.OfType<VisualElement>())
                {
                    Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(child);
                }
                foreach (var child in layout.Children.OfType<Label>())
                {
                    await child.FadeTo(0, 200, Easing.SinOut);
                }
                foreach (var child in layout.Children.OfType<Image>())
                {
                    await child.TranslateTo(0, 0, 200, Easing.SinOut);
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
