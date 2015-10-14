using System;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CoreTechs.Common
{
    public class AppSettings
    {
        private readonly IAppSettingsProvider _appSettingsProvider;

        public AppSettings(IAppSettingsProvider appSettingsProvider)
        {
            if (appSettingsProvider == null) throw new ArgumentNullException(nameof(appSettingsProvider));
            _appSettingsProvider = appSettingsProvider;
        }

        public AppSettings():this(new ConfigAppSettingsProvider())
        {
        }

        public string GetRequiredSetting([CallerMemberName]string name = null)
        {
            var setting = _appSettingsProvider[name];

            if (setting == null)
                throw new SettingsPropertyNotFoundException(
                    $"The setting with name '{name}' was not found.");

            return setting;
        }

        public T GetRequiredSetting<T>(Func<string, T> parser, [CallerMemberName] string name = null)
        {
            // setting must be defined in config
            Debug.Assert(name != null, "name != null");
            var setting = GetRequiredSetting(name);

            try
            {
                return parser(setting);
            }
            catch (Exception ex)
            {
                throw new FormatException(
                    $"The AppSetting '{name}' was not formatted correctly. The specified value was '{setting}'", ex);
            }
        }

        public string GetSettingOrDefault(string @default = null, [CallerMemberName] string name = null)
        {
            return _appSettingsProvider[name] ?? @default;
        }

        

        public T GetSettingOrDefault<T>(Func<string, T> parser, T @default = default(T),
            [CallerMemberName] string name = null)
        {
            try
            {
                return parser(_appSettingsProvider[name]);
            }
            catch
            {
                return @default;
            }
        }

        public TEnum GetAppSettingAsEnumOrDefault<TEnum>(TEnum @default = default (TEnum), bool ignoreCase = true,
            [CallerMemberName] string name = null) where TEnum : struct
        {
            var type = typeof(TEnum);
            if (!type.IsEnum)
                throw new ArgumentException($"'{type.FullName}' is not an enum type");

            Debug.Assert(name != null, "name != null");
            return GetSettingOrDefault(s => (TEnum)Enum.Parse(type, s, ignoreCase), @default, name);
        }
    }

    public class ConfigAppSettingsProvider : IAppSettingsProvider
    {

        public string this[string name]
        {
            get
            {
                if (name == null) throw new ArgumentNullException(nameof(name));
                return ConfigurationManager.AppSettings[name];
            }
        }
    }

    public interface IAppSettingsProvider
    {
        string this[string name] { get; }
    }
}