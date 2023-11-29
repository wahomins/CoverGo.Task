using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoverGo.Task.Domain;
public class CartRequestObject
{
    [Required]
    public required string customerId { get; init; }

    [Required]
    [StringLength(100, ErrorMessage = "Name cannot be Empty.")]
    public required string product { get; set; }

    [Required]
    [Range(1, double.MaxValue, ErrorMessage = "Quantity should be greater than zero")]
    public required int quantity { get; set; }

}

