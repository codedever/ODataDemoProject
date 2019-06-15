using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;

namespace ODataDemoProject.Models
{
    public static class TypeExtensions
    {
        /// <summary>
        /// 是否为可空类型
        /// </summary>
        /// <param name="type">待判断的类型</param>
        /// <returns>判断结果</returns>
        public static bool IsNullable(this Type type)
        {
            return !type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        /// <summary>
        /// 获取类型的核心类型, 用于判断可空类型的基本类型
        /// </summary>
        /// <param name="type">原始类型</param>
        /// <returns>核心类型</returns>
        public static Type GetCoreType(this Type type)
        {
            if (type != null && IsNullable(type))
            {
                return !type.IsValueType ? type : Nullable.GetUnderlyingType(type);
            }
            else
            {
                return type;
            }
        }

        /// <summary>
        /// 获取泛型类型的泛型参数类型
        /// </summary>
        /// <param name="type">原始类型</param>
        /// <returns>泛型参数类型</returns>
        public static Type GetGenericType(this Type type)
        {
            if (!type.IsGenericType)
            {
                Type baseType = type.BaseType;
                do
                {
                    baseType = baseType.BaseType;
                }
                while (baseType != null && baseType.BaseType != null && !baseType.IsGenericType);

                if (baseType != null && baseType.IsGenericType)
                {
                    type = baseType;
                }
            }

            if (type.IsGenericType)
            {
                return type.GenericTypeArguments.FirstOrDefault().GetCoreType();
            }

            return type.GetCoreType();
        }

        /// <summary>
        /// get the most inner base type of a type
        /// </summary>
        /// <param name="type">type instance</param>
        /// <returns>the most inner type</returns>
        public static Type GetBaseType(this Type type)
        {
            if (type.BaseType != null)
            {
                return type.BaseType.GetBaseType();
            }

            return type;
        }

        /// <summary>
        /// 反射公共构造方法获取实例
        /// </summary>
        /// <param name="type"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static object GetInstance(this Type type, params object[] parameters)
        {
            var constructorInfo = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(x => x.GetParameters().Count() == (parameters?.Length ?? 0));
            if (constructorInfo != null)
            {
                return constructorInfo.Invoke(parameters);
            }

            return null;
        }

        /// <summary>
        /// to judge a type is from type or inherit from type 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="from"></param>
        /// <returns>bool</returns>
        public static bool IsFromType(this Type type, Type from)
        {
            if (type == from || from.IsAssignableFrom(type) || type.IsSubclassOf(from))
            {
                return true;
            }

            if (type.BaseType != null)
            {
                return type.BaseType.IsFromType(from);
            }

            return false;
        }

        public static bool IsCollection(this Type type)
        {
            return type.IsFromType(typeof(IEnumerable));
        }

        public static bool IsGenericType(this Type type)
        {
            if (!type.IsGenericType)
            {
                do
                {
                    type = type.BaseType;
                }
                while (type.BaseType != null && !type.IsGenericType);
                return type.IsGenericType;
            }

            return true;
        }

        /// <summary>
        /// 判断成员中是否包含特性
        /// </summary>
        /// <typeparam name="T">类型参数, Attribute 类型</typeparam>
        /// <param name="member">带判断成员</param>
        /// <returns>判断结果</returns>
        public static bool HasAttribute<T>(this MemberInfo member) where T : Attribute
        {
            if (member != null)
            {
                return member.GetCustomAttribute<T>() != null;
            }

            return false;
        }

        /// <summary>
        /// 判断成员中是否包含特性
        /// </summary>
        /// <param name="member">带判断成员</param>
        /// <param name="types">包含的特性</param>
        /// <returns>判断结果</returns>
        public static bool HasAttribute(this MemberInfo member, params Type[] types)
        {
            if (member != null && types.IsNotNullOrEmpty())
            {
                for (int i = 0; i < types.Length; i++)
                {
                    if (member.GetCustomAttributes().Any(x => types.Contains(x.GetType())))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取对象上的显示名称(DisplayName特性的值)
        /// </summary>
        /// <param name="member">待获取对象</param>
        /// <returns>显示名称</returns>
        public static string GetDisplayName(this MemberInfo member)
        {
            string result = null;
            if (member != null)
            {
                var display = member.GetCustomAttribute<DisplayAttribute>();
                if (display != null)
                {
                    result = display.Name;
                }
                else
                {
                    var displayName = member.GetCustomAttribute<DisplayNameAttribute>();
                    if (displayName != null)
                    {
                        result = displayName.DisplayName;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 获取一个对象中对于类型 T 的引用的外键名称集合
        /// </summary>
        /// <typeparam name="T">类型参数</typeparam>
        /// <param name="type">包含T类型引用的对象</param>
        /// <returns>外键名集合</returns>
        public static List<string> GetForeignKeys<T>(this Type type) where T : EntitySet
        {
            if (type != null)
            {
                var properties = type.GetProperties().Where(x => x.PropertyType.IsFromType(typeof(T))).ToList();
                if (properties.IsNotNullOrEmpty())
                {
                    var list = new List<string>();
                    foreach (var prop in properties)
                    {
                        var fk = prop.GetCustomAttribute<ForeignKeyAttribute>();
                        if (fk != null)
                        {
                            list.Add(fk.Name);
                        }
                        else
                        {
                            list.Add(prop.Name + nameof(EntitySet.Id));
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取一个对象中对于类型 t 的引用的外键名称集合
        /// </summary>
        /// <param name="type">包含类型 t 引用的对象</param>
        /// <param name="t">类型 t</param>
        /// <returns>外键名集合</returns>
        public static List<string> GetForeignKeys(this Type type, Type t)
        {
            if (type != null)
            {
                var properties = type.GetProperties().Where(x => x.PropertyType.IsFromType(t)).ToList();
                if (properties.IsNotNullOrEmpty())
                {
                    var list = new List<string>();
                    foreach (var prop in properties)
                    {
                        var fk = prop.GetCustomAttribute<ForeignKeyAttribute>();
                        if (fk != null)
                        {
                            list.Add(fk.Name);
                        }
                        else
                        {
                            list.Add(prop.Name + nameof(EntitySet.Id));
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取对象obj中对于类型 T 的第一个引用的外键名称
        /// </summary>
        /// <typeparam name="T">类型参数</typeparam>
        /// <param name="type">包含类型 T 引用的对象</param>
        /// <returns>外键名</returns>
        public static string GetForeignKey<T>(this Type type) where T : class
        {
            if (type != null)
            {
                var prop = type.GetProperties().FirstOrDefault(x => x.PropertyType.IsFromType(typeof(T)));
                if (prop != null)
                {
                    var fk = prop.GetCustomAttribute<ForeignKeyAttribute>();
                    if (fk != null)
                    {
                        return fk.Name;
                    }

                    return prop.Name + nameof(EntitySet.Id);
                }
            }

            return null;
        }

        /// <summary>
        /// 获取对象obj中对于类型 t 的第一个引用的外键名称
        /// </summary>
        /// <param name="type">包含类型 t 引用的对象</param>
        /// <param name="t">类型 t</param>
        /// <returns>外键名</returns>
        public static string GetForeignKey(this Type type, Type t)
        {
            if (type != null)
            {
                var prop = type.GetProperties().FirstOrDefault(x => x.PropertyType.IsFromType(t));
                if (prop != null)
                {
                    var fk = prop.GetCustomAttribute<ForeignKeyAttribute>();
                    if (fk != null)
                    {
                        return fk.Name;
                    }

                    return prop.Name + nameof(EntitySet.Id);
                }
            }

            return null;
        }

        /// <summary>
        /// 根据外键属性获取导航属性
        /// 外键对于的导航属性为将外键设置为 ForeignKey 特性的属性或者末尾加上 Id 等于外键列名的属性
        /// </summary>
        /// <param name="property">外键属性</param>
        /// <returns>导航属性</returns>
        public static PropertyInfo GetForeignKeyNavigationProperty(this PropertyInfo property)
        {
            var type = property.DeclaringType;
            var prop = type.GetProperties().FirstOrDefault(x => x.GetCustomAttributes<ForeignKeyAttribute>().Any(a => a.Name == property.Name));
            if (prop == null)
            {
                prop = type.GetProperty(property.Name.Substring(0, property.Name.Length - nameof(EntitySet.Id).Length));
            }

            return prop;
        }

        /// <summary>
        /// 反射调用类型的静态泛型方法
        /// </summary>
        /// <param name="type">调用类型</param>
        /// <param name="methodName">方法名</param>
        /// <param name="parameters">参数列表</param>
        /// <param name="returnValue">返回值</param>
        /// <param name="genericTypes">泛型类型</param>
        public static void InvokeGenericMethod(this Type type, string methodName, object[] parameters, out object returnValue, params Type[] genericTypes)
        {
            returnValue = null;
            var methodInfo = type.GetMethods().FirstOrDefault(x => x.IsGenericMethod && x.Name == methodName && x.GetParameters().Count() == (parameters?.Length ?? 0));
            if (methodInfo != null)
            {
                returnValue = methodInfo.MakeGenericMethod(genericTypes).Invoke(null, parameters);
            }
        }

        /// <summary>
        /// 反射调用类型的静态泛型方法
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="type">调用类型</param>
        /// <param name="methodName">方法名</param>
        /// <param name="parameters">参数列表</param>
        /// <param name="genericTypes">泛型类型</param>
        /// <returns>返回调用结果</returns>
        public static T InvokeGenericMethod<T>(this Type type, string methodName, object[] parameters, params Type[] genericTypes) where T : class
        {
            var methodInfo = type.GetMethods().FirstOrDefault(x => x.IsGenericMethod && x.Name == methodName && x.GetParameters().Count() == (parameters?.Length ?? 0));
            if (methodInfo != null)
            {
                return (T)methodInfo.MakeGenericMethod(genericTypes).Invoke(null, parameters);
            }

            return default;
        }

        /// <summary>
        /// 反射调用类型的泛型扩展方法
        /// 预处理调用者参数, 因此不需要传递调用者参数
        /// </summary>
        /// <param name="type">调用类型</param>
        /// <param name="methodName">方法名</param>
        /// <param name="caller">调用对象</param>
        /// <param name="parameters">参数列表</param>
        /// <param name="returnValue">返回值</param>
        /// <param name="genericTypes">泛型类型</param>
        public static void InvokeGenericMethod(this Type type, string methodName, object caller, object[] parameters, out object returnValue, params Type[] genericTypes)
        {
            returnValue = null;
            var methodInfo = type.GetTypeInfo().GetDeclaredMethods(methodName).FirstOrDefault(x => x.IsGenericMethod && x.GetParameters().Count() == (parameters?.Length ?? 0) + 1);
            if (methodInfo != null)
            {
                var args = new List<object>();
                args.Add(caller);
                if (parameters.IsNotNullOrEmpty())
                {
                    args.AddRange(parameters);
                }

                returnValue = methodInfo.MakeGenericMethod(genericTypes).Invoke(caller, args.ToArray());
            }
        }

        /// <summary>
        /// 反射调用类型的泛型扩展方法
        /// 预处理调用者参数, 因此不需要传递调用者参数
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="type">调用类型</param>
        /// <param name="methodName">方法名</param>
        /// <param name="caller">调用对象</param>
        /// <param name="parameters">参数列表</param>
        /// <param name="genericTypes">泛型类型</param>
        /// <returns>返回调用结果</returns>
        public static T InvokeGenericMethod<T>(this Type type, string methodName, object caller, object[] parameters, params Type[] genericTypes) where T : class
        {
            var methodInfo = type.GetTypeInfo().GetDeclaredMethods(methodName).FirstOrDefault(x => x.IsGenericMethod && x.GetParameters().Count() == (parameters?.Length ?? 0) + 1);
            if (methodInfo != null)
            {
                var args = new List<object>();
                args.Add(caller);
                if (parameters.IsNotNullOrEmpty())
                {
                    args.AddRange(parameters);
                }

                return (T)methodInfo.MakeGenericMethod(genericTypes).Invoke(caller, args.ToArray());
            }

            return default;
        }
    }
}
