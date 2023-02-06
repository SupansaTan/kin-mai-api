using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinMai.EntityFramework.Models;

[Table("User")]
public partial class User
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(255)]
    public string FirstName { get; set; } = null!;

    [StringLength(255)]
    public string LastName { get; set; } = null!;

    [StringLength(255)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string? Email { get; set; }

    public DateTime CreateAt { get; set; }

    public int UserType { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<FavoriteRestaurant> FavoriteRestaurants { get; } = new List<FavoriteRestaurant>();

    [InverseProperty("Owner")]
    public virtual ICollection<Restaurant> Restaurants { get; } = new List<Restaurant>();

    [InverseProperty("User")]
    public virtual ICollection<Reviewer> Reviewers { get; } = new List<Reviewer>();
}
