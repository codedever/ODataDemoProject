using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ODataDemoProject.Models;
using System;
using System.Reflection;

namespace ODataDemoProject
{
    public static class Extensions
    {
        /// <summary>
        /// 尚不支持集合值
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private static void GetConfig(this IConfiguration configuration, string key, object value)
        {
            var type = value.GetType();
            if (!type.IsCollection() && !type.IsGenericType())
            {
                var propertyInfos = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                if (propertyInfos.IsNotNullOrEmpty())
                {
                    foreach (var propertyInfo in propertyInfos)
                    {
                        var subKey = key + AppSettings.DEFAULT_CONFIGURATION_SPLITER + propertyInfo.Name;
                        if (propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType == typeof(string))
                        {
                            var subValue = configuration.GetValue(propertyInfo.PropertyType, subKey);
                            if (subValue != null)
                            {
                                value.SetPropertyValue(propertyInfo.Name, subValue);
                            }
                        }
                        else
                        {
                            var subValue = propertyInfo.PropertyType.GetInstance(null);
                            if (subValue != null && !propertyInfo.PropertyType.IsCollection() && !propertyInfo.PropertyType.IsGenericType())
                            {
                                configuration.GetConfig(subKey, subValue);
                                value.SetPropertyValue(propertyInfo.Name, subValue);
                            }
                        }
                    }
                }
            }
        }

        internal static T GetConfig<T>(this IConfiguration configuration, string key) where T : class, new()
        {
            var t = new T();
            configuration.GetConfig(key, t);
            return t;
        }

        internal static void SetValue<T>(this ISession session, string key, T value)
        {
            session.SetString(key, value.ToJsonString());
        }

        internal static T GetValue<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            if (value == null)
            {
                return default;
            }

            return value.ToJsonObject<T>();
        }

        /// <summary>
        /// 使用依赖注入方式在项目启动时注入生成器所需的参数值;
        /// ASP.NET 中使用的服务注入方式;
        /// </summary>
        /// <param name="services">注入服务</param>
        /// <param name="action">生成器的参数构造器委托</param>
        /// <returns>服务本身</returns>
        internal static IServiceCollection AddKeyGenerator(this IServiceCollection services, Action<KeyGeneratorOptions> action)
        {
            KeyGenerator.InitOptions(action);
            return services;
        }
    }
}
