using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataDemoProject.Models
{
    [Cacheable]
    [Table("T_PRODUCT")]
    public class Product : EntitySet
    {
        public Product()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        [Required, Column("NAME", TypeName = "NVARCHAR(36)")]
        public string Name { get; set; }
        [Column("BRAND", TypeName = "NVARCHAR(36)")]
        public string Brand { get; set; }
        [Column("PRICE", TypeName = "DECIMAL(12, 4)")]
        public decimal Price { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
