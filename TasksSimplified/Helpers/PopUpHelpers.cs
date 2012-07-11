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
            var themeId = Settings.ThemeSetting == 0 ? Resource.Style.AlertThemeDark : Resource.Style.AlertThemeLight;
            var builder = new AlertDialog.Builder(new ContextThemeWrapper(context, themeId));
            builder.SetTitle(titleId);

            
            builder.SetItems(itemArray, (sender, args) => callback(args.Which));
            builder.SetCancelable(true);
            builder.SetNegativeButton(Resource.String.cancel, delegate { });

            var alertDialog = builder.Create();

            alertDialog.Show();
        }
    }
}