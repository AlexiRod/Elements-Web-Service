
using Newtonsoft.Json;

namespace ElementsMVCApp.Utilities
{
    public static class SessionExtensions
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.Indented
        };

        public static void Set<T>(this ISession session, string key, T value)
        {
            var t = JsonConvert.SerializeObject(value, settings);
            session.SetString(key, t);
        }

        public static T? Get<T>(this ISession session, string key)
        {
            string? value = session.GetString(key);
            var d = value == null ? default : JsonConvert.DeserializeObject<T>(value,
                new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            return value == null ? default : d;
        }
    }
}
