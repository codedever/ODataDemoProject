using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataDemoProject.Models
{
    [Cacheable]
    [Table("T_ORDER")]
    public class Order : EntitySet
    {
        public Order()
        {
            OrderDetails = new HashSet<OrderDetail>();
        }

        [Column("ORDER_USER_ID", TypeName = "BIGINT(19)")]
        public long OrderUserId { get; set; }
        [Column("PRICE", TypeName = "DECIMAL(12, 4)")]
        public decimal Price { get; set; }

        public virtual User OrderUser { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
