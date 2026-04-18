using HELIX.Widgets.Universal.Controllers;

namespace HELIX.Widgets.Utilities {
    public static class FocusHelper {
        public static void Focus(this GlobalKey key, bool navigated = false) {
            if (navigated) WidgetStateController.LastNavigated = true; // This should highlight the event once focused
            var element = key.Target?.Element;
            element?.schedule.Execute(element.Focus).ExecuteLater(1);
        }
        
        public static void Unfocus(this GlobalKey key) {
            var element = key.Target?.Element;
            element?.schedule.Execute(element.Blur).ExecuteLater(1);
        }
    }
}