using System.ComponentModel.DataAnnotations;

namespace Conference.API.Dtos;

public record AddSeatsRequest(
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")] int Quantity
);
