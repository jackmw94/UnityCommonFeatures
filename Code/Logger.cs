#if !DISABLE_LOGGING
#define ENABLE_LOGGING
#endif

using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace UnityCommonFeatures
{
    /// <summary>
    /// Wrapper around logs to allow us to turn them off for release builds
    /// Add DISABLE_LOGGING to compiler symbols to prevent log functions getting called
    ///
    /// I don't want to worry about logs or assertion predicates eating up milliseconds
    /// so need them to be fully compiled out in final versions
    ///
    /// I'm concerned that Debug.logger.logEnabled = false will still process logs and
    /// assertion predicates, so this is a safer solution
    /// </summary>
    public static class Logger
    {
        [Conditional("ENABLE_LOGGING")]
        public static void Log(string message)
        {
            Debug.Log(message);
        }
    
        [Conditional("ENABLE_LOGGING")]
        public static void Log(object obj)
        {
            Debug.Log(obj.ToString());
        }
    
        [Conditional("ENABLE_LOGGING")]
        public static void Log(string message, Object context)
        {
            Debug.Log(message, context);
        }

        [Conditional("ENABLE_LOGGING")]
        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }
    
        [Conditional("ENABLE_LOGGING")]
        public static void LogWarning(object obj)
        {
            Debug.LogWarning(obj.ToString());
        }
    
        [Conditional("ENABLE_LOGGING")]
        public static void LogWarning(string message, Object context)
        {
            Debug.LogWarning(message, context);
        }

        [Conditional("ENABLE_LOGGING")]
        public static void LogError(string message)
        {
            Debug.LogError(message);
        }
    
        [Conditional("ENABLE_LOGGING")]
        public static void LogError(object obj)
        {
            Debug.LogError(obj.ToString());
        }
    
        [Conditional("ENABLE_LOGGING")]
        public static void LogError(string message, Object context)
        {
            Debug.LogError(message, context);
        }

        [Conditional("ENABLE_LOGGING")]
        public static void Assert(Func<bool> predicate, string assertionFailMessage)
        {
            Debug.Assert(predicate(), assertionFailMessage);
        }

        [Conditional("ENABLE_LOGGING")]
        public static void Assert(bool assertValue, string assertionFailMessage)
        {
            Debug.Assert(assertValue, assertionFailMessage);
        }
    
        [Conditional("ENABLE_LOGGING")]
        public static void Assert(Func<bool> predicate, object assertionFailMessage)
        {
            Debug.Assert(predicate(), assertionFailMessage.ToString());
        }

        [Conditional("ENABLE_LOGGING")]
        public static void Assert(bool assertValue, object assertionFailMessage)
        {
            Debug.Assert(assertValue, assertionFailMessage.ToString());
        }
    
        [Conditional("ENABLE_LOGGING")]
        public static void Assert(Func<bool> predicate, Object context)
        {
            Debug.Assert(predicate(), context);
        }

        [Conditional("ENABLE_LOGGING")]
        public static void Assert(bool assertValue, Object context)
        {
            Debug.Assert(assertValue, context);
        }
    }
}