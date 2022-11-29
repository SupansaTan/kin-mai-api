using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinMai.EntityFramework.Models;

public partial class Category
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    public int Type { get; set; }

    [InverseProperty("Categories")]
    public virtual ICollection<Related> Relateds { get; } = new List<Related>();
}
