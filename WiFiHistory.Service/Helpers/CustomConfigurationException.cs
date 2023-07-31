using System;

namespace WiFiHistory.Service.Helpers
{
    [Serializable]
    public class CustomConfigurationException: Exception
    {
        public CustomConfigurationException() { }

        public CustomConfigurationException(string exception): base(exception) { }
    }
}
