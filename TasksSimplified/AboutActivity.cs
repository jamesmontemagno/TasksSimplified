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


using Android.App;
using Android.OS;
using Android.Preferences;
using TasksSimplified.Helpers;

namespace TasksSimplified
{
    [Activity(Label = "About", Icon = "@drawable/ic_launcher")]
    public class AboutActivity : PreferenceActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            SetTheme(Settings.ThemeSetting == 0 ? Resource.Style.MyTheme : Resource.Style.MyThemeDark);

            base.OnCreate(bundle);

            FlurryAgent.OnPageView();
            FlurryAgent.LogEvent("AboutPage");

            PreferenceManager.SharedPreferencesName = Settings.PrefName;
            AddPreferencesFromResource(Resource.Xml.preferences);
        }

        protected override void OnStart()
        {
            base.OnStart();
            FlurryAgent.OnStartSession(this, "TNGK6T6P75ZRSBV82QVF");
        }

        protected override void OnStop()
        {
            base.OnStop();
            FlurryAgent.OnEndSession(this);
        }
    }
}