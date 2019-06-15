using Newtonsoft.Json;
using System;

namespace ODataDemoProject.Models
{
    /// <summary>
    /// 超长数字类型转换器(超过13位数字转为字符串输出到客户端, 因为 js 的数字范围较小, 超过范围后会丢失精度)
    /// </summary>
    public class LargeNumberConverter : JsonConverter
    {
        /// <summary>
        /// 基准长度
        /// </summary>
        private const int _benchmarkLength = 13;
        private readonly int _length = 0;

        /// <summary>
        /// 超长数字类型转换器
        /// 超过 length 指定位数字转为字符串输出到客户端, 因为 js 的数字范围较小, 超过范围后会丢失精度
        /// length 不要超过 16
        /// length 默认值 13
        /// </summary>
        /// <param name="length"></param>
        public LargeNumberConverter(int length = _benchmarkLength)
        {
            if (length <= 0 || length >= _benchmarkLength)
            {
                length = _benchmarkLength;
            }

            _length = length;
        }

        /// <summary>
        /// 定义那些类型可以被转换
        /// </summary>
        /// <param name="objectType">可转换的类型, 包括 long, ulong, decimal, float 和 double</param>
        /// <returns>符合类型返回 true, 否则返回 false</returns>
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(long) || objectType == typeof(long?)
                || objectType == typeof(ulong) || objectType == typeof(ulong?)
                || objectType == typeof(decimal) || objectType == typeof(decimal?)
                || objectType == typeof(float) || objectType == typeof(float?)
                || objectType == typeof(double) || objectType == typeof(double?))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 重写输入规则, 将客户端传递的数字类型, 重新转化为原始类型
        /// </summary>
        /// <param name="reader">json 当前输入对象</param>
        /// <param name="objectType">原始类型</param>
        /// <param name="existingValue">已存在的值</param>
        /// <param name="serializer">序列化器</param>
        /// <returns>超长的数字已被转为字符串表示形式的现在重新转换为原始类型返回, 否则返回原值</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var valueType = objectType.GetCoreType();
            if (reader.Value != null && (reader.TokenType == JsonToken.String || reader.ValueType != valueType))
            {
                return Convert.ChangeType(reader.Value, valueType);
            }

            return reader.Value;
        }

        /// <summary>
        /// 重写输出规则, 将原始类型数字超过构造参数位数的转换为字符串表示形式输出
        /// </summary>
        /// <param name="writer">json 当前输出对象</param>
        /// <param name="value">当前值</param>
        /// <param name="serializer">序列化器</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value != null && value.ToString().Length > _length)
            {
                value = value.ToString();
            }

            writer.WriteValue(value);
        }
    }
}
