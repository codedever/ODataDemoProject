using System;

namespace ODataDemoProject.Models
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheableAttribute : Attribute
    {
        /// <summary>
        /// 是否需要缓存到客户端
        /// </summary>
        public bool LocalCache { get; }
        /// <summary>
        /// 当前缓存的依赖关系
        /// </summary>
        public string[] Relationships { get; }

        /// <summary>
        /// 使用依赖关系初始化缓存特性
        /// </summary>
        /// <param name="relationships"></param>
        public CacheableAttribute(params string[] relationships)
        {
            Relationships = relationships;
        }

        /// <summary>
        /// 使用是否缓存到客户端, 以及依赖关系初始化缓存
        /// </summary>
        /// <param name="localCache"></param>
        /// <param name="relationships"></param>
        public CacheableAttribute(bool localCache = false, params string[] relationships)
        {
            LocalCache = localCache;
            Relationships = relationships;
        }
    }
}
