using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinMai.EntityFramework.Models;

[Table("Reviewer")]
public partial class Reviewer
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid RestaurantId { get; set; }

    public int CountStar { get; set; }

    [StringLength(255)]
    public string? Comment { get; set; }

    public string[]? ImageLink { get; set; }

    public string[]? FoodRecommendList { get; set; }

    public DateTime CreateAt { get; set; }

    [ForeignKey("RestaurantId")]
    [InverseProperty("Reviewers")]
    public virtual Restaurant Restaurant { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Reviewers")]
    public virtual User User { get; set; } = null!;
}
