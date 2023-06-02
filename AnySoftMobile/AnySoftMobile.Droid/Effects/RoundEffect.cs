using Android.Graphics;
using Android.Views;
using Java.IO;
using Java.Lang;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Rect = Android.Graphics.Rect;

[assembly: ResolutionGroupName("Xamarin")]
[assembly: ExportEffect(typeof(AnySoftMobile.Droid.Effects.RoundEffect), nameof(AnySoftMobile.Droid.Effects.RoundEffect))]
namespace AnySoftMobile.Droid.Effects;

public class RoundEffect : PlatformEffect
{
    ViewOutlineProvider originalProvider;
    Android.Views.View effectTarget;

    protected override void OnAttached()
    {
        try
        {
            effectTarget = Control ?? Container;
            originalProvider = effectTarget.OutlineProvider;
            effectTarget.OutlineProvider = new CornerRadiusOutlineProvider(Element);
            effectTarget.ClipToOutline = true;
        }
        catch (Exception ex)
        {
        }
    }

    protected override void OnDetached()
    {
        if (effectTarget != null)
        {
            effectTarget.OutlineProvider = originalProvider;
            effectTarget.ClipToOutline = false;
        }
    }

    class CornerRadiusOutlineProvider : ViewOutlineProvider
    {
        Element element;

        public CornerRadiusOutlineProvider(Element formsElement)
        {
            element = formsElement;
        }

        public override void GetOutline(Android.Views.View view, Outline outline)
        {
            float scale = view.Resources.DisplayMetrics.Density;
            double width = (double)element.GetValue(VisualElement.WidthProperty) * scale;
            double height = (double)element.GetValue(VisualElement.HeightProperty) * scale;
            float minDimension = (float)Math.Min(height, width);
            float radius = minDimension / 2f;
            Rect rect = new Rect(0, 0, (int)width, (int)height);
            outline.SetRoundRect(rect, radius);
        }
    }
}