using MediaOrganizerApp.Application;
using System;
using System.Threading;

namespace MediaOrganizerApp.WebApi.Services
{
    public static class FeatureFlagsStore
    {
        private static FeatureFlags _flags = new FeatureFlags();
        private static readonly ReaderWriterLockSlim _lock = new();

        // Raised when flags are updated
        public static event Action<FeatureFlags>? Changed;

        public static FeatureFlags Get()
        {
            _lock.EnterReadLock();
            try { return new FeatureFlags { AiBadgeEnabled = _flags.AiBadgeEnabled, AiFallbackEnabled = _flags.AiFallbackEnabled }; }
            finally { _lock.ExitReadLock(); }
        }

        public static void Set(FeatureFlags flags)
        {
            _lock.EnterWriteLock();
            try
            {
                _flags = flags ?? new FeatureFlags();
            }
            finally { _lock.ExitWriteLock(); }

            // notify listeners (do not hold lock while invoking)
            try
            {
                Changed?.Invoke(Get());
            }
            catch
            {
                // swallow exceptions from listeners
            }
        }
    }
}
