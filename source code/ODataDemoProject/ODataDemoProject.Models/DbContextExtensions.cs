using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;

namespace ODataDemoProject.Models
{
    public static class DbContextExtensions
    {
        public static string GetContextUserId(this DbContext dbContext)
        {
            var httpContext = dbContext.GetNonPublicValue<HttpContext>();
            if (httpContext?.User?.Identity?.IsAuthenticated ?? false)
            {
                return httpContext?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            }

            return null;
        }

        public static User GetContextUser(this DbContext dbContext)
        {
            User user = null;
            var httpContext = dbContext.GetNonPublicValue<HttpContext>();
            var cache = dbContext.GetNonPublicValue<IDistributedCache>();
            if (httpContext != null && (httpContext?.User?.Identity?.IsAuthenticated ?? false) && cache != null)
            {
                var userId = dbContext.GetContextUserId();
                var cacheKey = BaseAppSettings.HTTP_CONTEXT_USER + userId;
                user = cache.GetValue<User>(cacheKey);
                if (user == null)
                {
                    user = dbContext.GetQueryable<User>().FirstOrDefault(x => x.Id.ToString() == userId);
                    if (user != null)
                    {
                        user.Password = null;
                        cache.SetValue(cacheKey, user);
                    }
                }
            }

            return user;
        }

        /// <summary>
        /// 统一数据源查询入口, 可缓存数据源
        /// </summary>
        /// <typeparam name="TEntitySet"></typeparam>
        /// <param name="dbContext"></param>
        /// <param name="enableCache"></param>
        /// <returns>返回对当前类型的数据库上下文的引用</returns>
        public static IQueryable<TEntitySet> GetQueryable<TEntitySet>(this DbContext dbContext) where TEntitySet : class
        {
            var type = typeof(TEntitySet);
            if (type.HasAttribute<CacheableAttribute>())
            {
                var cacheKey = BaseAppSettings.DATA_CACHE + type.Name.ToUpper();
                var cache = dbContext.GetNonPublicValue<IDistributedCache>();
                if (cache != null)
                {
                    var entitySets = cache.GetValue<IEnumerable<TEntitySet>>(cacheKey);
                    if (entitySets == null)
                    {
                        entitySets = dbContext.Set<TEntitySet>().ToHashSet();
                        cache.SetValue(cacheKey, entitySets);
                    }

                    return entitySets.AsQueryable();
                }
            }

            return dbContext.Set<TEntitySet>().AsQueryable();
        }

        public static void ReloadCache<TEntitySet>(this DbContext dbContext) where TEntitySet : EntitySet
        {
            var type = typeof(TEntitySet);
            if (type.HasAttribute<CacheableAttribute>())
            {
                var cacheKey = BaseAppSettings.DATA_CACHE + type.Name.ToUpper();
                var _cache = dbContext.GetNonPublicValue<IDistributedCache>();
                if (_cache != null)
                {
                    _cache.Remove(cacheKey);
                    _cache.SetValue(cacheKey, dbContext.Set<TEntitySet>().ToHashSet());
                }
            }
        }

        public static HttpStatusCode AddToSaveChange<TEntitySet>(this DbContext dbContext, TEntitySet entitySet) where TEntitySet : EntitySet
        {
            if (entitySet == null)
            {
                return HttpStatusCode.BadRequest;
            }

            dbContext.Set<TEntitySet>().Add(entitySet);
            try
            {
                dbContext.SaveChanges();
                dbContext.ReloadCache<TEntitySet>();
            }
            catch (DbUpdateException)
            {
                if (dbContext.Set<TEntitySet>().Any(x => x.Id.Equals(entitySet.Id)))
                {
                    return HttpStatusCode.Conflict;
                }
                else
                {
                    throw;
                }
            }

            return HttpStatusCode.OK;
        }

        public static HttpStatusCode UpdateToSaveChange<TEntitySet>(this DbContext dbContext, long id, TEntitySet entitySet) where TEntitySet : EntitySet
        {
            if (!id.Equals(entitySet.Id))
            {
                return HttpStatusCode.BadRequest;
            }

            dbContext.Entry(entitySet).State = EntityState.Modified;

            //if (entitySet.State == 0)
            //{
            //    dbContext.Entry(entitySet).Property(nameof(entitySet.State)).IsModified = false;
            //}

            //if (!entitySet.CreatedOn.HasValue)
            //{
            //    dbContext.Entry(entitySet).Property(nameof(entitySet.CreatedOn)).IsModified = false;
            //}

            //if (entitySet.CreatedBy == default)
            //{
            //    dbContext.Entry(entitySet).Property(nameof(entitySet.CreatedBy)).IsModified = false;
            //}

            //if (entitySet.Description.IsNullOrEmpty())
            //{
            //    dbContext.Entry(entitySet).Property(nameof(entitySet.Description)).IsModified = false;
            //}

            try
            {
                dbContext.SaveChanges();
                dbContext.ReloadCache<TEntitySet>();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (dbContext.Set<TEntitySet>().Any(x => x.Id.Equals(id)))
                {
                    throw ex;
                }
                else
                {
                    return HttpStatusCode.NotFound;
                }
            }

            return HttpStatusCode.OK;
        }

        public static HttpStatusCode DeleteToSaveChange<TEntitySet>(this DbContext dbContext, TEntitySet entitySet) where TEntitySet : EntitySet
        {
            if (entitySet == null)
            {
                return HttpStatusCode.NotFound;
            }

            dbContext.Set<TEntitySet>().Remove(entitySet);
            dbContext.SaveChanges();
            dbContext.ReloadCache<TEntitySet>();
            return HttpStatusCode.OK;
        }
    }
}
