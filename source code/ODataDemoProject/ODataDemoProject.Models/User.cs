using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataDemoProject.Models
{
    [Cacheable]
    [Table("T_USER")]
    public class User : EntitySet
    {
        public User()
        {
            Orders = new HashSet<Order>();
        }

        [Required, Column("NAME", TypeName = "NVARCHAR(36)")]
        public string Name { get; set; }
        [Column("USER_NAME", TypeName = "VARCHAR(36)")]
        public string UserName { get; set; }
        [Column("PASSWORD", TypeName = "VARCHAR(128)")]
        public string Password { get; set; }

        public virtual ICollection<Order> Orders { get; set; }
    }
}
