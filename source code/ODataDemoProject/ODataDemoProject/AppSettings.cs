using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using ODataDemoProject.Models;
using System;

namespace ODataDemoProject
{
    public class AppSettings
    {
        public const string DEVELOPER_USER_NAME = "developer";
        public const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const long SYSTEM_USER_ID = 3;
        /// <summary>
        /// 默认数据状态
        /// </summary>
        public const long DATA_STATE_AVAILABLE = 101001;
        /// <summary>
        /// 默认连接符
        /// </summary>
        public const string DEFAULT_SEPARATOR = ".";
        /// <summary>
        /// 默认分隔符
        /// </summary>
        public const string DEFAULT_SPLITER = ",";
        /// <summary>
        /// 默认连接符
        /// </summary>
        public const string DEFAULT_CONNECTOR = "_";
        /// <summary>
        /// 默认路径分隔符
        /// </summary>
        public const string DEFAULT_PATH_SPLITER = "/";
        /// <summary>
        /// 默认配置文件分隔符
        /// </summary>
        public const string DEFAULT_CONFIGURATION_SPLITER = ":";
        public const string DEFAULT_DATE_FORMAT = "yyyy-MM-dd";
        public const string DEFAULT_DATETIME_FORMAT = "yyyy-MM-dd HH:mm:ss";
        public const string ORACLE_DB_DATETIME_FORMAT = "yyyyMMddHHmmss";
        public const string APP_SETTINGS_FILE = "appsettings";
        public const string APP_SETTINGS_FILE_EXTENSION = "json";
        public const string DEFAULT_APP_SETTINGS_FILE = "appsettings.json";
        public const string HOSTS = "AppSetting:Hosts";
        public const string REDIS_INSTANCE_NAME = "DEMO_";
        /// <summary>
        /// 用于存放数据库数据缓存的前缀
        /// </summary>
        public const string DATA_CACHE = "DATA_CACHE_";
        /// <summary>
        /// 用于存放 application 全局处理相关的缓存前缀
        /// </summary>
        public const string APPLICATION_CACHE = "APP_CACHE_";
        /// <summary>
        /// 用于存放当前用户 session 级别的缓存的前缀
        /// </summary>
        public const string SESSION_CACHE = "SESSION_CACHE_";
        public const string SESSION_CACHE_SESSION_ID = SESSION_CACHE + "SESSION_ID_";
        public const string HTTP_CONTEXT_USER = APPLICATION_CACHE + "HTTP_CONTEXT_USER_";
        /// <summary>
        /// 默认附件路径
        /// </summary>
        public const string DEFAULT_ATTACHMENT_PATH = "/Attachments/";
        public const int DEFAULT_MAX_UPLOAD_FILE_SIZE = 10 * 1024 * 1024;
        public const int DEFAULT_TOTAL_UPLOAD_FILE_SIZE = 1024 * 1024 * 1024;
        public const string DEFAULT_ALLOW_FILE_EXTENSIONS = ".png,.jpg,.jpeg,.gif,.bmp,.xls,.xlsx,.doc,.docx,.ppt,.pptx,.pdf,.txt";

        public static AppSetting AppSetting { get; internal set; }
        public static KeyGeneratorOptions KeyGenerator { get; internal set; }

        public static readonly JsonSerializerSettings JsonSerializerSetting = new JsonSerializerSettings()
        {
            //忽略循环引用
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //使用驼峰样式的key
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            //设置时间格式
            DateTimeZoneHandling = DateTimeZoneHandling.Local,
            DateParseHandling = DateParseHandling.DateTime,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateFormatString = DEFAULT_DATETIME_FORMAT
        };

        static AppSettings()
        {
            //枚举类型转字符串输出
            JsonSerializerSetting.Converters.Add(new StringEnumConverter());
            //超长数字转字符串输出
            JsonSerializerSetting.Converters.Add(new LargeNumberConverter());
        }

        public static Action<JsonSerializerSettings> SetupJsonSerializerSetting = (serializerSettings) =>
        {
            //忽略循环引用
            serializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            //使用驼峰样式的key
            serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            //设置时间格式
            serializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Local;
            serializerSettings.DateParseHandling = DateParseHandling.DateTime;
            serializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            serializerSettings.DateFormatString = DEFAULT_DATETIME_FORMAT;
            //枚举类型转字符串输出
            serializerSettings.Converters.Add(new StringEnumConverter());
            //超长数字转字符串输出
            serializerSettings.Converters.Add(new LargeNumberConverter());
        };
    }

    public class AppSetting
    {
        public AppSetting()
        {
            AttachmentsPath = AppSettings.DEFAULT_ATTACHMENT_PATH;
            MaxFileSize = AppSettings.DEFAULT_MAX_UPLOAD_FILE_SIZE;
            TotalFileSize = AppSettings.DEFAULT_TOTAL_UPLOAD_FILE_SIZE;
            AllowFileExtensions = AppSettings.DEFAULT_ALLOW_FILE_EXTENSIONS;
        }

        public string Hosts { get; set; }
        public string CorsOrigins { get; set; }
        public bool EnableCache { get; set; }
        public string AttachmentsPath { get; set; }
        public int MaxFileSize { get; set; }
        public int TotalFileSize { get; set; }
        public string AllowFileExtensions { get; set; }
    }

    public class EndPoint
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }
}
