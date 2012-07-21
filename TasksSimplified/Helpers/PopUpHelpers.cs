using System;
using Android.App;
using Android.Content;
using Android.Views;

namespace TasksSimplified.Helpers
{
    public static class PopUpHelpers
    {
        public static void ShowListPopup(Context context, int titleId, int itemArray, Action<int> callback)
        {
            var themeId = Settings.DarkTheme ? Resource.Style.AlertThemeDark : Resource.Style.AlertThemeLight;
            var builder = new AlertDialog.Builder(new ContextThemeWrapper(context, themeId));
            builder.SetTitle(titleId);
            builder.SetIcon(Resource.Drawable.ic_launcher);
            
            builder.SetItems(itemArray, (sender, args) => callback(args.Which));
            builder.SetCancelable(true);
            builder.SetNegativeButton(Resource.String.cancel, delegate { });

            var alertDialog = builder.Create();

            alertDialog.Show();
        }

        public static void ShowOKCancelPopup(Context context, int titleId, int messageId, Action<bool> callback)
        {
            var themeId = Settings.DarkTheme? Resource.Style.AlertThemeDark : Resource.Style.AlertThemeLight;
            var builder = new AlertDialog.Builder(new ContextThemeWrapper(context, themeId));
            builder.SetIcon(Resource.Drawable.ic_launcher);
            builder.SetTitle(titleId);
            builder.SetMessage(messageId);
            builder.SetCancelable(true);
            builder.SetNegativeButton(Resource.String.cancel, delegate { callback(false); });//do nothign on cancel

            builder.SetPositiveButton(Resource.String.ok, delegate
            {
                callback(true);
            });

            var alertDialog = builder.Create();
            alertDialog.Show();
        }

        public static void ShowOKPopup(Context context, int titleId, int messageId, Action<bool> callback)
        {
            var themeId = Settings.DarkTheme? Resource.Style.AlertThemeDark : Resource.Style.AlertThemeLight;
            var builder = new AlertDialog.Builder(new ContextThemeWrapper(context, themeId));
            builder.SetIcon(Resource.Drawable.ic_launcher);
            builder.SetTitle(titleId);
            builder.SetMessage(messageId);
            builder.SetPositiveButton(Resource.String.ok, delegate
            {
                callback(true);
            });

            var alertDialog = builder.Create();
            alertDialog.Show();
        }
    }
}