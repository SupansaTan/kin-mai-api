using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinMai.EntityFramework.Models;

[Table("FavoriteRestaurant")]
public partial class FavoriteRestaurant
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid RestaurantId { get; set; }

    public DateTime CreateAt { get; set; }

    [ForeignKey("RestaurantId")]
    [InverseProperty("FavoriteRestaurants")]
    public virtual Restaurant Restaurant { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("FavoriteRestaurants")]
    public virtual User User { get; set; } = null!;
}
