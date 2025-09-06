namespace MCOCSrv.Resources.Popups
{
    public class PopupBase : ContentView
    {
        public string BorderStroke = "RoundRectangle 10";
        public void Show()
        {
            this.InputTransparent = false;
            this.IsVisible = true;
            Animations.Animations.FadeInAnimation(this);
        }

        public async Task Hide()
        {
            this.InputTransparent = true;
            await Animations.Animations.FadeOutAnimation(this);
            this.IsVisible = false;
        }


    }
}
