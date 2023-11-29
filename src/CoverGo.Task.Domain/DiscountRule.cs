using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoverGo.Task.Domain;

public class DiscountRule
{
    [Required]
    [StringLength(100, ErrorMessage = "Name cannot be Empty.")]
    public required string productName { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public required int forEvery { get; set; }
}
