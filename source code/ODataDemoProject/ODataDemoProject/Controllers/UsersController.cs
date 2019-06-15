using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using ODataDemoProject.Models;

namespace ODataDemoProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Get(ODataQueryOptions<User> options)
        {
            return this.Get<User>(options);
        }
    }
}