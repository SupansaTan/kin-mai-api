﻿using System;
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

    public string[]? ImageLink { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public DateTime CreateAt { get; set; }

    public int[] DeliveryType { get; set; } = null!;

    public int[] PaymentMethod { get; set; } = null!;

    public int RestaurantType { get; set; }

    [InverseProperty("Restaurant")]
    public virtual ICollection<BusinessHour> BusinessHours { get; } = new List<BusinessHour>();

    [ForeignKey("OwnerId")]
    [InverseProperty("Restaurants")]
    public virtual User Owner { get; set; } = null!;

    [InverseProperty("Restaurant")]
    public virtual ICollection<Related> Relateds { get; } = new List<Related>();

    [InverseProperty("Restaurant")]
    public virtual ICollection<Reviewer> Reviewers { get; } = new List<Reviewer>();

    [InverseProperty("Restaurant")]
    public virtual ICollection<SocialContact> SocialContacts { get; } = new List<SocialContact>();
}