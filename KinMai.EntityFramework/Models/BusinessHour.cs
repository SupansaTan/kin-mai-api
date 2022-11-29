using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace KinMai.EntityFramework.Models;

public partial class BusinessHour
{
    [Key]
    public Guid Id { get; set; }

    public Guid RestaurantId { get; set; }

    public int Day { get; set; }

    public TimeOnly OpenTime { get; set; }

    public TimeOnly CloseTime { get; set; }

    [ForeignKey("RestaurantId")]
    [InverseProperty("BusinessHours")]
    public virtual Restaurant Restaurant { get; set; } = null!;
}
