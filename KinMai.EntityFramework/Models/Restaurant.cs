using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinMai.EntityFramework.Models;

[Table("Restaurant")]
public partial class Restaurant
{
    [Key]
    public Guid Id { get; set; }

    public Guid OwnerId { get; set; }

    [StringLength(255)]
    public string Name { get; set; } = null!;

    [NotMapped]
    public string[]? ImageLink { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public DateTime CreateAt { get; set; }

    [NotMapped]
    public int[]? DeliveryType { get; set; }

    [NotMapped]
    public int[]? PaymentMethod { get; set; }

    public int RestaurantType { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public int MinPriceRate { get; set; }

    public int MaxPriceRate { get; set; }

    [InverseProperty("Restaurant")]
    public virtual ICollection<BusinessHour> BusinessHours { get; } = new List<BusinessHour>();

    [InverseProperty("Restaurant")]
    public virtual ICollection<FavoriteRestaurant> FavoriteRestaurants { get; } = new List<FavoriteRestaurant>();

    [ForeignKey("OwnerId")]
    [InverseProperty("Restaurants")]
    public virtual User Owner { get; set; } = null!;

    [InverseProperty("Restaurant")]
    public virtual ICollection<Related> Relateds { get; } = new List<Related>();

    [InverseProperty("Restaurant")]
    public virtual ICollection<Review> Reviews { get; } = new List<Review>();

    [InverseProperty("Restaurant")]
    public virtual ICollection<SocialContact> SocialContacts { get; } = new List<SocialContact>();
}
