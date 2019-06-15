using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataDemoProject.Models
{
    public class EntitySet
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Column("ID", TypeName = "BIGINT(19)")]
        public virtual long Id { get; set; }
        [ConcurrencyCheck, Column("ROW_VERSION", TypeName = "BIGINT(19)")]
        public virtual long RowVersion { get; set; }
        [Column("STATE", TypeName = "BIGINT(19)")]
        public virtual long State { get; set; }
        [Column("CREATED_ON", TypeName = "DATETIME")]
        public virtual DateTime? CreatedOn { get; set; }
        [Column("CREATED_BY", TypeName = "VARCHAR(36)")]
        public virtual string CreatedBy { get; set; }
        [Column("UPDATED_ON", TypeName = "DATETIME")]
        public virtual DateTime? UpdatedOn { get; set; }
        [Column("UPDATED_BY", TypeName = "VARCHAR(36)")]
        public virtual string UpdatedBy { get; set; }
        [Column("DESCRIPTION", TypeName = "VARCHAR(256)")]
        public virtual string Description { get; set; }

        public virtual long GetKey()
        {
            return KeyGenerator.GetKey();
        }

        public virtual void Create()
        {
            if (Id == 0)
            {
                Id = GetKey();
                RowVersion = GetKey();
            }

            CreatedOn = CreatedOn ?? DateTime.Now;
        }

        public virtual void Update()
        {
            RowVersion = GetKey();
            UpdatedOn = UpdatedOn ?? DateTime.Now;
        }
    }
}
