using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CoverGo.Task.Domain;

public class Product
{

    [Required]
    [StringLength(100, ErrorMessage = "Name cannot be Empty.")]
    public required string Name { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative.")]
    public required decimal Price { get; set; }

    [JsonIgnore]
    public string Currency { get; set; } = "HKD";
}
