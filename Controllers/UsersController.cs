using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using calcul_arrima.Data;
using calcul_arrima.Models;
using calcul_arrima.DTOs;

namespace calcul_arrima.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        
        var response = users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email,
            Nationality = u.Nationality,
            TotalScore = u.TotalScore,
            CreatedAt = u.CreatedAt
        });

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        var response = new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Nationality = user.Nationality,
            TotalScore = user.TotalScore,
            CreatedAt = user.CreatedAt
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> CreateUser(UserCreateDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(userDto.FirstName) || string.IsNullOrWhiteSpace(userDto.LastName) || string.IsNullOrWhiteSpace(userDto.Email))
        {
            return BadRequest("Fields cannot be empty or contain only whitespace.");
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email);
        if (existingUser != null)
        {
            return Conflict("A user with this email already exists.");
        }

        var user = new User
        {
            FirstName = userDto.FirstName.Trim(),
            LastName = userDto.LastName.Trim(),
            Email = userDto.Email.Trim(),
            Nationality = userDto.Nationality,
            TotalScore = userDto.TotalScore,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var response = new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Nationality = user.Nationality,
            TotalScore = user.TotalScore,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, response);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, UserUpdateDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(userDto.FirstName) || string.IsNullOrWhiteSpace(userDto.LastName) || string.IsNullOrWhiteSpace(userDto.Email))
        {
            return BadRequest("Fields cannot be empty or contain only whitespace.");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == userDto.Email && u.Id != id);
        if (existingUser != null)
        {
            return Conflict("A user with this email already exists.");
        }

        user.FirstName = userDto.FirstName.Trim();
        user.LastName = userDto.LastName.Trim();
        user.Email = userDto.Email.Trim();
        user.Nationality = userDto.Nationality;
        user.TotalScore = userDto.TotalScore;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await UserExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private async Task<bool> UserExists(int id)
    {
        return await _context.Users.AnyAsync(e => e.Id == id);
    }
}