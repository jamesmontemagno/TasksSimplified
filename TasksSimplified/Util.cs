using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace TasksSimplified
{
    public static class Util
    {
        public static void ShowOKCancelPopup(Context context, int titleId, int messageId, Action<bool> callback)
        {
            var builder = new AlertDialog.Builder(context);

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
    }
}