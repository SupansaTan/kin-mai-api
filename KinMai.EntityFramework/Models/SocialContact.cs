using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinMai.EntityFramework.Models;

[Table("SocialContact")]
public partial class SocialContact
{
    [Key]
    public Guid Id { get; set; }

    public Guid RestaurantId { get; set; }

    public int SocialType { get; set; }

    [StringLength(255)]
    public string ContactValue { get; set; } = null!;

    [ForeignKey("RestaurantId")]
    [InverseProperty("SocialContacts")]
    public virtual Restaurant Restaurant { get; set; } = null!;
}
