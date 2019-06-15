using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ODataDemoProject.Models;
using System.Linq;

namespace ODataDemoProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public OrdersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [EnableQuery]
        public IActionResult Get()
        {
            return Ok(_context.GetQueryable<Order>());
        }

        [HttpPost]
        public IActionResult Seed()
        {
            using (_context)
            {
                var userId = AppSettings.SYSTEM_USER_ID.ToString();
                var computer = new Product { Name = "主机", Brand = "联想", Price = 3000, Description = "联想主机", CreatedBy = userId, UpdatedBy = userId };
                var cpu = new Product { Name = "CPU", Brand = "Intel", Price = 1200, Description = "I5 8400", CreatedBy = userId, UpdatedBy = userId };
                var mainBoard = new Product { Name = "主板", Brand = "技嘉", Price = 900, Description = "技嘉主板", CreatedBy = userId, UpdatedBy = userId };
                var memory = new Product { Name = "内存", Brand = "金士顿", Price = 700, Description = "16GB DDR4 2400M", CreatedBy = userId, UpdatedBy = userId };
                var ssd = new Product { Name = "SSD", Brand = "Intel", Price = 300, Description = "Intel 760P", CreatedBy = userId, UpdatedBy = userId };
                _context.AddToSaveChange(computer);
                _context.AddToSaveChange(cpu);
                _context.AddToSaveChange(mainBoard);
                _context.AddToSaveChange(memory);
                _context.AddToSaveChange(ssd);

                var mayun = new User { Name = "马云", UserName = "mayun", Description = "马云", CreatedBy = userId, UpdatedBy = userId };
                var mahuateng = new User { Name = "马化腾", UserName = "mahuateng", Description = "马化腾", CreatedBy = userId, UpdatedBy = userId };
                _context.AddToSaveChange(mayun);
                _context.AddToSaveChange(mahuateng);
                var userIdMayun = mayun.Id.ToString();
                var userIdMahuateng = mahuateng.Id.ToString();

                var mayunOrder = new Order { OrderUserId = mayun.Id, Description = "马云下的单", CreatedBy = userIdMayun, UpdatedBy = userIdMayun };
                var mahuatengOrder1 = new Order { OrderUserId = mahuateng.Id, Description = "马化腾下的单", CreatedBy = userIdMahuateng, UpdatedBy = userIdMahuateng };
                var mahuatengOrder2 = new Order { OrderUserId = mahuateng.Id, Description = "马化腾下的单", CreatedBy = userIdMahuateng, UpdatedBy = userIdMahuateng };
                _context.AddToSaveChange(mayunOrder);
                _context.AddToSaveChange(mahuatengOrder1);
                _context.AddToSaveChange(mahuatengOrder2);

                _context.AddToSaveChange(new OrderDetail { ProductId = computer.Id, OrderId = mayunOrder.Id, Count = 1, Description = "马云下的单", CreatedBy = userIdMayun, UpdatedBy = userIdMayun });
                _context.AddToSaveChange(new OrderDetail { ProductId = cpu.Id, OrderId = mahuatengOrder1.Id, Count = 1, Description = "马化腾下的单", CreatedBy = userIdMahuateng, UpdatedBy = userIdMahuateng });
                _context.AddToSaveChange(new OrderDetail { ProductId = mainBoard.Id, OrderId = mahuatengOrder2.Id, Count = 1, Description = "马化腾下的单", CreatedBy = userIdMahuateng, UpdatedBy = userIdMahuateng });
                _context.AddToSaveChange(new OrderDetail { ProductId = memory.Id, OrderId = mahuatengOrder2.Id, Count = 1, Description = "马化腾下的单", CreatedBy = userIdMahuateng, UpdatedBy = userIdMahuateng });
                _context.AddToSaveChange(new OrderDetail { ProductId = memory.Id, OrderId = mahuatengOrder2.Id, Count = 2, Description = "马化腾下的单", CreatedBy = userIdMahuateng, UpdatedBy = userIdMahuateng });
                _context.AddToSaveChange(new OrderDetail { ProductId = ssd.Id, OrderId = mahuatengOrder2.Id, Count = 2, Description = "马化腾下的单", CreatedBy = userIdMahuateng, UpdatedBy = userIdMahuateng });

                mayunOrder.Price = _context.GetQueryable<OrderDetail>().Include(nameof(OrderDetail.Product)).Where(x => x.OrderId == mayunOrder.Id).Sum(x => x.Product.Price * x.Count);
                mahuatengOrder1.Price = _context.GetQueryable<OrderDetail>().Include(nameof(OrderDetail.Product)).Where(x => x.OrderId == mahuatengOrder1.Id).Sum(x => x.Product.Price * x.Count);
                mahuatengOrder2.Price = _context.GetQueryable<OrderDetail>().Include(nameof(OrderDetail.Product)).Where(x => x.OrderId == mahuatengOrder2.Id).Sum(x => x.Product.Price * x.Count);
                _context.UpdateToSaveChange(mayunOrder.Id, mayunOrder);
                _context.UpdateToSaveChange(mahuatengOrder1.Id, mahuatengOrder1);
                _context.UpdateToSaveChange(mahuatengOrder2.Id, mahuatengOrder2);
            }

            return Ok();
        }
    }
}