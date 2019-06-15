using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace ODataDemoProject.Models
{
    public partial class ApplicationDbContext : DbContext
    {
        private readonly IDistributedCache _cache;
        private readonly HttpContext _httpContext;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            IDistributedCache cache,
            IHttpContextAccessor httpContext)
            : base(options)
        {
            _cache = cache;
            _httpContext = httpContext?.HttpContext;
        }

        #region entity models
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>().Property(x => x.Name).IsUnicode();
            modelBuilder.Entity<Product>().Property(x => x.Brand).IsUnicode();
            modelBuilder.Entity<Product>().Property(x => x.Description).IsUnicode();
            modelBuilder.Entity<User>().Property(x => x.Name).IsUnicode();
            modelBuilder.Entity<User>().Property(x => x.Description).IsUnicode();
            modelBuilder.Entity<Order>().Property(x => x.Description).IsUnicode();
            modelBuilder.Entity<OrderDetail>().Property(x => x.Description).IsUnicode();
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            SetupEntitySet();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetupEntitySet();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetupEntitySet()
        {
            if (ChangeTracker.HasChanges())
            {
                var userId = _httpContext?.User?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;

                //处理数据访问操作基类字段
                foreach (var entity in ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged))
                {
                    if (entity.Entity is EntitySet entitySet)
                    {
                        if (entity.State == EntityState.Added)
                        {
                            entitySet.Create();
                            entitySet.State = BaseAppSettings.DATA_STATE_AVAILABLE;
                            entitySet.CreatedBy = entitySet.CreatedBy ?? userId;
                        }

                        if (entity.State == EntityState.Added || entity.State == EntityState.Modified)
                        {
                            entitySet.Update();
                            entitySet.UpdatedBy = entitySet.UpdatedBy ?? userId;
                        }
                    }
                }
            }
        }
    }
}
