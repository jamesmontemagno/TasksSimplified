using Android.App;
using Android.Content;
using TasksSimplified.DataAccessLayer;

namespace TasksSimplified.Helpers
{
    public static class Settings
    {
        public static readonly string PrefName = "TASKSSIMPLIFIEDSETTINGS";
        private const string ThemeKey = "ThemeSetting";
        private const string ThemeAccentKey = "ThemeAccent";
        private const string TalkBackKey = "TalkBack";
        private const string KeepKeyboardUpKey = "KeepKeyboardUp";
        private const string SortByKey = "SortBy";


        private static readonly ISharedPreferences SharedPreferences;
        private static readonly ISharedPreferencesEditor SharedPreferencesEditor;


        static Settings()
        {
            SharedPreferences = Application.Context.GetSharedPreferences(PrefName, FileCreationMode.Private);
            SharedPreferencesEditor = SharedPreferences.Edit();
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

        public static int ThemeAccent
        {
            get
            {
                var returnValue = SharedPreferences.GetInt(ThemeAccentKey, 2);
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
                switch(ThemeAccent)
                {
                    case 0:
                        return Resource.Color.actionbar_maincolor_blue;
                    case 1:
                        return Resource.Color.actionbar_maincolor_gray;
                    case 2:
                        return Resource.Color.actionbar_maincolor_green;
                    case 3:
                        return Resource.Color.actionbar_maincolor_purple;
                    case 4:
                        return Resource.Color.actionbar_maincolor_red;
                     case 5:
                        return Resource.Color.actionbar_maincolor_yellow;
                }

                return Resource.Color.actionbar_maincolor_green;
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