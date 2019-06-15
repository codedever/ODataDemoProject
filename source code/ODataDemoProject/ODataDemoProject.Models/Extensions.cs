using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ODataDemoProject.Models
{
    public static class Extensions
    {
        public static string ToJsonString<T>(this T t)
        {
            try
            {
                return JsonConvert.SerializeObject(t, BaseAppSettings.JsonSerializerSetting);
            }
            catch (Exception ex)
            {
                throw new Exception($"the object of type {typeof(T).Name} can not be serialize! {ex.Message}");
            }
        }

        public static T ToJsonObject<T>(this string value)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(value, BaseAppSettings.JsonSerializerSetting);
            }
            catch (Exception ex)
            {
                throw new Exception($"the value of type {typeof(T).Name} can not be deserialize! {ex.Message}");
            }
        }

        /// <summary>
        /// 将类型 Source 的对象复制到 Target 的对象, Source 和 Target 需为兼容类型(即需要有相同字段)
        /// </summary>
        /// <typeparam name="Source">原始类型</typeparam>
        /// <typeparam name="Target">目标类型</typeparam>
        /// <param name="source">原始类型对象</param>
        /// <returns>复制得到的目标类型对象</returns>
        public static Target Copy<Source, Target>(this Source source)
        {
            try
            {
                return source.ToJsonString().ToJsonObject<Target>();
            }
            catch (Exception ex)
            {
                throw new Exception($"type of {typeof(Source).Name} can not be copy to type of {typeof(Target)}! {ex.Message}");
            }
        }

        /// <summary>
        /// 复制对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="t">原始类型对象</param>
        /// <returns>复制得到的目标类型对象</returns>
        public static T Copy<T>(this T t)
        {
            try
            {
                return t.ToJsonString().ToJsonObject<T>();
            }
            catch (Exception ex)
            {
                throw new Exception($"type of {typeof(T).Name} can not be copy! {ex.Message}");
            }
        }

        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static bool IsNotNullOrEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }

        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }

        public static bool IsCollection(this object obj)
        {
            return obj.GetType().IsFromType(typeof(IEnumerable));
        }

        public static string ToHexString(this IEnumerable<byte> bytes, string spliter = " ")
        {
            if (bytes.IsNotNullOrEmpty())
            {
                var value = BitConverter.ToString(bytes.ToArray());
                if (spliter != "-")
                {
                    value = value.Replace("-", spliter);
                }

                return value;
            }

            return null;
        }

        public static string GetHashCode<THashAlgorithm>(this string value) where THashAlgorithm : HashAlgorithm
        {
            if (value.IsNotNullOrEmpty())
            {
                var hashAlgorithm = (THashAlgorithm)typeof(THashAlgorithm).InvokeMember(nameof(HashAlgorithm.Create), BindingFlags.Static | BindingFlags.Public | BindingFlags.InvokeMethod, null, null, null);
                if (hashAlgorithm != null)
                {
                    return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(value))?.ToHexString("");
                }
            }

            return null;
        }

        public static byte[] ToBinary(this string hexString, string spliter = " ")
        {
            if (!string.IsNullOrEmpty(hexString))
            {
                if (string.IsNullOrEmpty(spliter))
                {
                    var bytes = new byte[hexString.Length / 2];
                    for (int i = 0; i < hexString.Length; i += 2)
                    {
                        bytes[i] = Convert.ToByte(hexString.Substring(i, 2), 16);
                    }

                    return bytes;
                }
                else
                {
                    var binaryArray = hexString.Split(spliter);
                    if (binaryArray.Length > 0)
                    {
                        var bytes = new byte[binaryArray.Length];
                        for (int i = 0; i < binaryArray.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(binaryArray[i]))
                            {
                                bytes[i] = Convert.ToByte(binaryArray[i], 16);
                            }
                        }

                        return bytes;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取枚举的可显示名字
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetDisplayName<T>(this object obj) where T : Enum
        {
            var result = string.Empty;
            if (obj != null)
            {
                var objectType = obj.GetType();
                if (objectType.IsEnum)
                {
                    var field = objectType.GetField(obj.ToString());
                    if (field != null)
                    {
                        var display = field.GetCustomAttribute<DisplayAttribute>();
                        if (display != null)
                        {
                            result = display.Name;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// get value by property
        /// </summary>
        /// <param name="obj">The object that have the property</param>
        /// <param name="property">property name, support the format of "A.B.C"</param>
        /// <returns></returns>
        public static object GetPropertyValue(this object obj, string property)
        {
            if (obj != null && !string.IsNullOrEmpty(property))
            {
                if (obj.GetType().IsFromType(typeof(IEnumerable)))
                {
                    //(obj as ICollection).Cast<object>().ToList().ForEach(x => { x = x.GetPropertyValue(current); });
                    object[] array = (obj as IEnumerable).Cast<object>().ToArray();
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i] = array[i].GetPropertyValue(property);
                    }

                    return array;
                }
                else
                {
                    int position = property.IndexOf('.');
                    string current = position > 0 ? property.Substring(0, position) : property;
                    PropertyInfo prop = obj.GetType().GetProperty(current);
                    if (prop != null)
                    {
                        obj = prop.GetValue(obj);
                        if (obj != null && position > 0)
                        {
                            string next = property.Substring(position + 1);
                            obj = obj.GetPropertyValue(next);
                        }

                        return obj;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 为对象的属性 property 设置 value 值
        /// </summary>
        /// <param name="obj">要设置属性的对象</param>
        /// <param name="property">属性名称</param>
        /// <param name="value">要设置的值</param>
        /// <returns>是否设置成功</returns>
        public static bool SetPropertyValue(this object obj, string property, object value)
        {
            if (obj != null && property.IsNotNullOrEmpty())
            {
                var prop = obj.GetType().GetProperty(property);
                if (prop != null)
                {
                    prop.SetValue(obj, value);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Collection ToString extension method, concatenate all collection object to a format string 
        /// </summary>
        /// <typeparam name="T">collection object type</typeparam>
        /// <param name="collection">collection object instance</param>
        /// <param name="format">object format, default is "{0}"</param>
        /// <param name="spliter">object spliter, default is "; "</param>
        /// <returns>concatenate all collection object to a format string</returns>
        public static string ToString<T>(this IEnumerable<T> collection, string format = "{0}", string spliter = "; ")
        {
            var result = new List<string>();
            if (collection != null)
            {
                foreach (var x in collection)
                {
                    result.Add(string.Format(format, x));
                }
            }

            return string.Join(spliter, result);
        }

        /// <summary>
        /// Key 自动生成器, 用于自动产生的连续的不重复的 Key 值
        /// </summary>
        /// <typeparam name="TKey">要生成的 Key 的类型</typeparam>
        /// <param name="key">类型实例, 该类型的任意值</param>
        /// <returns>自动产生的连续的不重复的 Key 值</returns>
        public static TKey GetKey<TKey>(this EntitySet entitySet) where TKey : IConvertible
        {
            if (entitySet == null)
            {
                throw new ArgumentNullException(nameof(entitySet));
            }

            var keyType = typeof(TKey);
            if (typeof(long) == keyType || typeof(string) == keyType)
            {
                return (TKey)Convert.ChangeType(KeyGenerator.GetKey(), keyType);
            }

            return default;
        }

        public static void SetValue<T>(this IDistributedCache cache, string key, T value)
        {
            cache.SetString(key, value.ToJsonString());
        }

        public static T GetValue<T>(this IDistributedCache cache, string key)
        {
            var value = cache.GetString(key);
            if (value == null)
            {
                return default;
            }

            return value.ToJsonObject<T>();
        }

        /// <summary>
        /// 反射调用对象的泛型方法
        /// </summary>
        /// <param name="caller">调用对象</param>
        /// <param name="methodName">方法名</param>
        /// <param name="parameters">参数列表</param>
        /// <param name="returnValue">返回值</param>
        /// <param name="genericTypes">泛型类型</param>
        public static void InvokeGenericMethod(this object caller, string methodName, object[] parameters, out object returnValue, params Type[] genericTypes)
        {
            returnValue = null;
            var methodInfo = caller.GetType().GetMethods().FirstOrDefault(x => x.IsGenericMethod && x.Name == methodName && x.GetParameters().Count() == (parameters?.Length ?? 0));
            if (methodInfo != null)
            {
                returnValue = methodInfo.MakeGenericMethod(genericTypes).Invoke(caller, parameters);
            }
        }

        /// <summary>
        /// 反射调用对象的泛型方法
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="caller">调用对象</param>
        /// <param name="methodName">方法名</param>
        /// <param name="parameters">参数列表</param>
        /// <param name="genericTypes">泛型类型</param>
        /// <returns>返回调用结果</returns>
        public static T InvokeGenericMethod<T>(this object caller, string methodName, object[] parameters, params Type[] genericTypes) where T : class
        {
            var methodInfo = caller.GetType().GetMethods().FirstOrDefault(x => x.IsGenericMethod && x.Name == methodName && x.GetParameters().Count() == (parameters?.Length ?? 0));
            if (methodInfo != null)
            {
                return (T)methodInfo.MakeGenericMethod(genericTypes).Invoke(caller, parameters);
            }

            return default;
        }

        public static T GetNonPublicValue<T>(this object obj, string fieldName = null)
        {
            var fieldInfo = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)?.FirstOrDefault(x => x.Name == fieldName || x.FieldType.IsFromType(typeof(T)));
            T t = default;
            if (fieldInfo != null)
            {
                t = (T)fieldInfo.GetValue(obj);
            }

            return t;
        }
    }
}
