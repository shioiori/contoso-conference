using System.ComponentModel.DataAnnotations;

namespace Eventbox.EventManagement.EventApi.Requests;

public class AddCapacityRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
    public int Quantity { get; set; }
}
