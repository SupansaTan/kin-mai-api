using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinMai.EntityFramework.Models;

[Table("Related")]
public partial class Related
{
    [Key]
    public Guid Id { get; set; }

    public Guid RestaurantId { get; set; }

    public Guid CategoriesId { get; set; }

    [ForeignKey("CategoriesId")]
    [InverseProperty("Relateds")]
    public virtual Category Categories { get; set; } = null!;

    [ForeignKey("RestaurantId")]
    [InverseProperty("Relateds")]
    public virtual Restaurant Restaurant { get; set; } = null!;
}
