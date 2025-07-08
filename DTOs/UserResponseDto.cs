using calcul_arrima.Models;

namespace calcul_arrima.DTOs;

public class UserResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Nationality Nationality { get; set; }
    public double TotalScore { get; set; }
    public DateTime CreatedAt { get; set; }
}