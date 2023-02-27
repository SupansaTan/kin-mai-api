using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinMai.EntityFramework.Models;

[Table("Review")]
public partial class Review
{
    [Key]
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid RestaurantId { get; set; }

    public int Rating { get; set; }

    [StringLength(255)]
    public string? Comment { get; set; }

    public string[]? ImageLink { get; set; }

    public string[]? FoodRecommendList { get; set; }

    public DateTime CreateAt { get; set; }

    public int[]? ReviewLabelRecommend { get; set; }

    [StringLength(255)]
    public string? ReplyComment { get; set; }

    [ForeignKey("RestaurantId")]
    [InverseProperty("Reviews")]
    public virtual Restaurant Restaurant { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Reviews")]
    public virtual User User { get; set; } = null!;
}
