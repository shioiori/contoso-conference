using System.ComponentModel.DataAnnotations;

namespace Eventbox.EventManagement.EventApi.Dtos;

public class AddSeatsRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}
