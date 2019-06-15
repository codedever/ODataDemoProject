using System.ComponentModel.DataAnnotations.Schema;

namespace ODataDemoProject.Models
{
    [Cacheable]
    [Table("T_ORDER_DETAIL")]
    public class OrderDetail : EntitySet
    {
        [Column("ORDER_ID", TypeName = "BIGINT(19)")]
        public long OrderId { get; set; }
        [Column("PRODUCT_ID", TypeName = "BIGINT(19)")]
        public long ProductId { get; set; }
        [Column("Count", TypeName = "INT")]
        public int Count { get; set; }

        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }
}
