using Android.App;
using Android.Content;
using TasksSimplified.DataAccessLayer;

namespace TasksSimplified.Helpers
{
    public static class Settings
    {
        public static readonly string PrefName = "TASKSSIMPLIFIEDSETTINGS";
        private const string ThemeKey = "ThemeSetting2";
        private const string ThemeAccentKey = "ThemeAccent2";
        private const string TalkBackKey = "TalkBack";
        private const string KeepKeyboardUpKey = "KeepKeyboardUp";
        private const string SortByKey = "SortBy";

        private const string CurrentVersionNumberKey = "CurrentVersionNumber";


        private static readonly ISharedPreferences SharedPreferences;
        private static readonly ISharedPreferencesEditor SharedPreferencesEditor;


        static Settings()
        {
            SharedPreferences = Application.Context.GetSharedPreferences(PrefName, FileCreationMode.Private);
            SharedPreferencesEditor = SharedPreferences.Edit();
        }


        public static bool DarkTheme
        {
            get { return ThemeSetting == 1; }
        }

        public static bool UseLightIcons
        {
            get { return ThemeAccent == 0; }
        }

        public static bool UseLightIconsBottom
        {
            get { return ThemeAccent == 0 || (ThemeAccent == 3 && !DarkTheme); }
        }

        public static int ThemeSetting
        {
            get
            {
                var returnValue = SharedPreferences.GetInt(ThemeKey, 0);
                if (returnValue < 0)
                    returnValue = 0;

                return returnValue;
            }
            set
            {
                SharedPreferencesEditor.PutInt(ThemeKey, value);
                SharedPreferencesEditor.Commit();
            }
        }

        public static int ThemeAccent
        {
            get
            {
                var returnValue = SharedPreferences.GetInt(ThemeAccentKey, 0);
                if (returnValue < 0)
                    returnValue = 0;

                return returnValue;
            }
            set
            {
                SharedPreferencesEditor.PutInt(ThemeAccentKey, value);
                SharedPreferencesEditor.Commit();
            }
        }

        public static int ThemeAccentId
        {
            get
            {
                switch (ThemeAccent)
                {
                    case 0:
                        return Resource.Color.actionbar_maincolor_lightgray;
                    case 1:
                        return Resource.Color.actionbar_maincolor;
                    case 2:
                        return Resource.Color.actionbar_maincolor_blue;
                    case 3:
                        return Resource.Color.actionbar_maincolor_black;
                }

                return Resource.Color.actionbar_maincolor;
            }
        }

        public static int CheckedColor
        {
            get
            {
                switch (ThemeAccent)
                {
                    case 0:
                        return Resource.Color.actionbar_pressedcolor_lightgray;
                    case 1:
                        return Resource.Color.actionbar_pressedcolor_darkgray;
                    case 2:
                        return Resource.Color.actionbar_pressedcolor_blue;
                    case 3:
                        return Resource.Color.actionbar_pressedcolor_black;
                }

                return Resource.Color.pager_darkgray;
            }
        }

        public static int PagerBackground
        {
            get
            {
                switch (ThemeAccent)
                {
                    case 0:
                        return Resource.Color.pager_lightgray;
                    case 1:
                        return Resource.Color.pager_darkgray;
                    case 2:
                        return Resource.Color.pager_blue;
                    case 3:
                        return Resource.Color.pager_black;
                }

                return Resource.Color.pager_darkgray;
            }
        }

        public static int PagerBackgroundText
        {
            get
            {
                switch (ThemeAccent)
                {
                    case 0:
                        return Resource.Color.actionbar_title_blue;
                    case 1:
                        return Resource.Color.actionbar_title;
                    case 2:
                        return Resource.Color.actionbar_title_blue;
                    case 3:
                        return Resource.Color.actionbar_title_black;
                }

                return Resource.Color.actionbar_title;
            }
        }

        public static int ImageButtonDrawable
        {
            get
            {
                switch (ThemeAccent)
                {
                    case 0:
                        return Resource.Drawable.clearbutton_lightgray;
                    case 1:
                        return Resource.Drawable.clearbutton;
                    case 2:
                        return Resource.Drawable.clearbutton_blue;
                    case 3:
                        return Resource.Drawable.clearbutton_black;
                }

                return Resource.Drawable.clearbutton;
            }
        }

        public static SortOption SortBy
        {
            get
            {
                var returnValue = SharedPreferences.GetInt(SortByKey, 0);
                if (returnValue < 0)
                    returnValue = 0;

                return (SortOption)returnValue;
            }
            set
            {
                SharedPreferencesEditor.PutInt(SortByKey, (int)value);
                SharedPreferencesEditor.Commit();
            }
        }

        public static string CurrentVersionNumber
        {
            get
            {
                return SharedPreferences.GetString(CurrentVersionNumberKey, "Unknown");
              
            }
            set
            {
                SharedPreferencesEditor.PutString(CurrentVersionNumberKey, value);
                SharedPreferencesEditor.Commit();
            }
        }

        public static bool TalkBackEnabled
        {
            get { return SharedPreferences.GetBoolean(TalkBackKey, true); }
            set
            {
                SharedPreferencesEditor.PutBoolean(TalkBackKey, value);
                SharedPreferencesEditor.Commit();
            }
        }

        public static bool KeepKeyboardUp
        {
            get { return SharedPreferences.GetBoolean(KeepKeyboardUpKey, true); }
            set
            {
                SharedPreferencesEditor.PutBoolean(KeepKeyboardUpKey, value);
                SharedPreferencesEditor.Commit();
            }
        }

    }
}