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
using Android.Content;
using Android.Runtime;

namespace TasksSimplified.Helpers
{

    public static class FlurryAgent
    {
        private static IntPtr m_FlurryClass = JNIEnv.FindClass("com/flurry/android/FlurryAgent");
        public static void OnStartSession(Context context, string appKey)
        {
#if DEBUG
#else

            IntPtr Flurry_onStartSession = JNIEnv.GetStaticMethodID(m_FlurryClass, "onStartSession", "(Landroid/content/Context;Ljava/lang/String;)V");
            Java.Lang.String key = new Java.Lang.String(appKey);
            JNIEnv.CallStaticVoidMethod(m_FlurryClass, Flurry_onStartSession, new JValue(context), new JValue(key));
#endif
        }

        public static void OnPageView()
        {
#if DEBUG
#else
            IntPtr Flurry_onPageView = JNIEnv.GetStaticMethodID(m_FlurryClass, "onPageView", "()V");
            JNIEnv.CallStaticVoidMethod(m_FlurryClass, Flurry_onPageView);
#endif
        }

        public static void OnEndSession(Context context)
        {
#if DEBUG
#else
            IntPtr Flurry_onEndSession = JNIEnv.GetStaticMethodID(m_FlurryClass, "onEndSession", "(Landroid/content/Context;)V");
            JNIEnv.CallStaticVoidMethod(m_FlurryClass, Flurry_onEndSession, new JValue(context));
#endif
        }

        public static void LogEvent(string eventName)
        {

#if DEBUG
#else
            IntPtr _flurryLogEvent = JNIEnv.GetStaticMethodID(m_FlurryClass, "logEvent", "(Ljava/lang/String;)V");
            JNIEnv.CallStaticVoidMethod(m_FlurryClass, _flurryLogEvent, new JValue(new Java.Lang.String(eventName)));
#endif
        }
    }
}