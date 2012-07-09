using Android.App;
using Android.Content;

namespace TasksSimplified.Helpers
{
    public static class Settings
    {
        public static readonly string PrefName = "TASKSSIMPLIFIEDSETTINGS";
        private const string ThemeKey = "ThemeSetting";
        private const string TalkBackKey = "TalkBack";
        private const string KeepKeyboardUpKey = "KeepKeyboardUp";


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