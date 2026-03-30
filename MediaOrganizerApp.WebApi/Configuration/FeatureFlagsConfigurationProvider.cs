using Microsoft.Extensions.Configuration;
using MediaOrganizerApp.WebApi.Services;
using MediaOrganizerApp.Application;

namespace MediaOrganizerApp.WebApi.Configuration
{
    // Configuration source wrapper
    public class FeatureFlagsConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new FeatureFlagsConfigurationProvider();
        }
    }

    // Provider that reads values from FeatureFlagsStore and raises reload when store changes
    public class FeatureFlagsConfigurationProvider : ConfigurationProvider, IDisposable
    {
        public FeatureFlagsConfigurationProvider()
        {
            Load();
            // subscribe to store changes so we can reload configuration
            FeatureFlagsStore.Changed += OnFlagsChanged;
        }

        private void OnFlagsChanged(FeatureFlags flags)
        {
            if (flags is null) return;

            // set values using the same keys as the conventional configuration section
            Data["Features:AiFallbackEnabled"] = flags.AiFallbackEnabled.ToString();
            Data["Features:AiBadgeEnabled"] = flags.AiBadgeEnabled.ToString();

            // notify configuration system listeners (IOptionsMonitor will receive change)
            OnReload();
        }

        public override void Load()
        {
            var flags = FeatureFlagsStore.Get() ?? new FeatureFlags();
            Data["Features:AiFallbackEnabled"] = flags.AiFallbackEnabled.ToString();
            Data["Features:AiBadgeEnabled"] = flags.AiBadgeEnabled.ToString();
        }

        public void Dispose()
        {
            FeatureFlagsStore.Changed -= OnFlagsChanged;
        }
    }
}
