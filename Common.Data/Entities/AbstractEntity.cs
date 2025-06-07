using System.ComponentModel.DataAnnotations;

namespace Common.Data.Entities;

public abstract class AbstractEntity
{
    [Key]
    public int Id { get; set; }


    public AbstractEntity()
    {
    }
}
