using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using ODataDemoProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ODataDemoProject
{
    public static class AspNetExtensions
    {
        /// <summary>
        /// 获取查询选项, 将来自 HttpPost 的 OData 查询选项加入到 QueryString 键值对中
        /// </summary>
        /// <param name="request">WebApi 的 HttpRequest 对象</param>
        /// <returns>返回合并后的查询选项</returns>
        internal static IEnumerable<KeyValuePair<string, StringValues>> GetQueryOptions(this HttpRequest request)
        {
            var queryString = request.Query.ToList();
            if (request.HasFormContentType && (request?.Form?.Keys?.Count ?? 0) > 0)
            {
                foreach (var key in request.Form.Keys)
                {
                    if (!queryString.Any(x => x.Key == key))
                    {
                        queryString.Add(request.Form.FirstOrDefault());
                    }
                }
            }

            return queryString;
        }

        internal static T GetQueryOption<T>(this HttpRequest request, string key) where T : IConvertible
        {
            var value = string.Empty;
            if (request.Query.Keys.Contains(key))
            {
                value = request.Query[key].FirstOrDefault();
            }
            else if (request.HasFormContentType && request.Form.Keys.IsNotNullOrEmpty() && request.Form.Keys.Contains(key))
            {
                value = request.Form[key].FirstOrDefault();
            }

            if (value.IsNotNullOrEmpty())
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }

            return default;
        }

        internal static ODataConventionModelBuilder BuildEdmModel<TContext>(this ODataConventionModelBuilder builder) where TContext : DbContext
        {
            var dbsets = typeof(TContext).GetProperties().Where(x => x.PropertyType.IsGenericType && x.PropertyType.GenericTypeArguments.FirstOrDefault().IsSubclassOf(typeof(EntitySet)));
            if (dbsets.Any())
            {
                foreach (var dbset in dbsets)
                {
                    var methodInfo = builder.GetType().GetMethod(nameof(builder.EntitySet));
                    var propertyType = dbset.PropertyType.GenericTypeArguments.First();
                    if (propertyType.GetCustomAttribute<NotMappedAttribute>() == null)
                    {
                        methodInfo = methodInfo.MakeGenericMethod(propertyType);
                        methodInfo.Invoke(builder, new object[] { dbset.Name });
                    }
                }
            }

            return builder;
        }

        internal static string GetSessionUserId(this HttpContext httpContext)
        {
            return httpContext?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
        }

        internal static string GetSessionUserId(this ControllerBase controller)
        {
            return controller?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
        }

        /// <summary>
        /// 
        /// 如有必要可引入以下特性跟踪代码错误位置
        /// [CallerFilePath]string fileName = ""
        /// [CallerLineNumber]int lineNumber = 0
        /// [CallerMemberName]string methodName = ""
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        private static DbContext GetDbContext(this ControllerBase controller)
        {
            var fieldInfo = controller.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)?.FirstOrDefault(x => x.FieldType.IsSubclassOf(typeof(DbContext)));
            DbContext dbContext = null;
            if (fieldInfo != null)
            {
                dbContext = fieldInfo.GetValue(controller) as DbContext;
            }

            if (dbContext == null)
            {
                throw new Exception("the property which is assignable from type DbContext in controller not found!");
            }

            return dbContext;
        }

        private static TService GetService<TService>(this ControllerBase controller)
        {
            var fieldInfo = controller.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)?.FirstOrDefault(x => x.FieldType.IsFromType(typeof(TService)));
            TService service = default;
            if (fieldInfo != null)
            {
                service = (TService)fieldInfo.GetValue(controller);
            }

            if (service == null)
            {
                throw new Exception($"the property which is assignable from type {typeof(TService)} in controller not found!");
            }

            return service;
        }

        public static string GetContextUserId(this ControllerBase controller)
        {
            if (controller?.User?.Identity?.IsAuthenticated ?? false)
            {
                return controller?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            }

            return controller?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
        }

        /// <summary>
        /// 正常情况下请使用此方法, 将来可再此注入缓存机制
        /// </summary>
        /// <typeparam name="TEntitySet"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="controller"></param>
        /// <returns></returns>
        internal static IQueryable<TEntitySet> GetQueryable<TEntitySet>(this ControllerBase controller) where TEntitySet : EntitySet
        {
            var dbContext = controller.GetDbContext();
            var queryable = dbContext.GetQueryable<TEntitySet>();
            if (true)
            {
                //...全局查询过滤
            }

            return queryable;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TEntitySet"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="controller"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        internal static IActionResult Get<TEntitySet>(this ControllerBase controller, ODataQueryOptions<TEntitySet> options, IQueryable<TEntitySet> queryable = null) where TEntitySet : EntitySet
        {
            queryable = queryable ?? controller.GetQueryable<TEntitySet>();
            var actionResult = new ODataActionResult<TEntitySet>();
            actionResult.Value = options.ApplyTo(queryable).Cast<object>().ToList();
            if (controller.Request.Query.Any(x => x.Key == "$count"))
            {
                var queries = controller.Request.Query;
                var noPagingQueries = QueryString.Empty;
                foreach (var query in queries)
                {
                    if (query.Key != "$top" && query.Key != "$skip")
                    {
                        noPagingQueries += QueryString.Create(query.Key, query.Value);
                    }
                }

                controller.Request.QueryString = noPagingQueries;
                var opts = new ODataQueryOptions<TEntitySet>(options.Context, controller.Request);
                actionResult.Count = opts.ApplyTo(queryable).Count();
            }

            return controller.Ok(actionResult);
        }

        internal static async Task<ActionResult<TEntitySet>> GetAsync<TEntitySet>(this ControllerBase controller, long id) where TEntitySet : EntitySet
        {
            var entitySet = await controller.GetQueryable<TEntitySet>().FirstOrDefaultAsync(x => x.Id == id);
            if (entitySet == null)
            {
                return new NotFoundResult();
            }

            return entitySet;
        }
    }
}
