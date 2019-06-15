using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using ODataDemoProject.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace ODataDemoProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Get(ODataQueryOptions<Product> options)
        {
            return this.Get<Product>(options);
        }

        [HttpGet("GetByOrderUser/{id}")]
        public IActionResult GetByOrderUser(ODataQueryOptions<Product> options, long id)
        {
            var queryable = _context.GetQueryable<OrderDetail>().Where(x => x.Order.OrderUserId == id).Select(x => x.Product).Distinct();
            return this.Get(options, queryable);
        }
    }
}