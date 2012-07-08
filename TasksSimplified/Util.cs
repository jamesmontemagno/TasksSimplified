/*
 * Copyright (C) 2012 James Montemagno (http://www.montemagno.com)
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *    
 *          http://www.apache.org/licenses/LICENSE-2.0
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/


using System;
using Android.App;
using Android.Content;

namespace TasksSimplified
{
    public static class Util
    {
        public static void ShowOkCancelPopup(Context context, int titleId, int messageId, Action<bool> callback)
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