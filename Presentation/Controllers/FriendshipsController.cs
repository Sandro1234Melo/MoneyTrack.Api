using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrack.Api.Application.Dtos.Users;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Infrastructure.Data;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController]
    [Route("api/friendships")]
    public class FriendshipsController : ControllerBase
    {
        private readonly MoneyTrackDbContext _context;
        public FriendshipsController(MoneyTrackDbContext context) => _context = context;

        private int UserId()
        {
            if (Request.Headers.TryGetValue("X-User-Id", out var value) && int.TryParse(value, out var id)) return id;
            throw new UnauthorizedAccessException("Usuário não autenticado.");
        }
        private IActionResult Error(Exception ex, int status = 400) => StatusCode(status, new { error = ex.Message });

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            try
            {
                var userId = UserId();
                q = q?.Trim() ?? string.Empty;
                if (q.Length < 2) return Ok(Array.Empty<object>());
                var users = await _context.Users.Where(u => u.Id != userId &&
                    (EF.Functions.ILike(u.FullName, $"%{q}%") || EF.Functions.ILike(u.Email, $"%{q}%")))
                    .OrderBy(u => u.FullName).Take(10)
                    .Select(u => new { u.Id, fullName = u.FullName, u.Email, profileImageUrl = u.ProfileImageUrl }).ToListAsync();
                return Ok(users);
            }
            catch (UnauthorizedAccessException ex) { return Error(ex, 401); }
            catch (Exception ex) { return Error(ex); }
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var userId = UserId();
                var relations = await _context.Friendships.Where(f => (f.SenderId == userId || f.ReceiverId == userId) && f.Status == FriendshipStatus.Accepted).ToListAsync();
                var friendIds = relations.Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId).ToList();
                var users = await _context.Users.Where(u => friendIds.Contains(u.Id)).Select(u => new { u.Id, fullName = u.FullName, u.Email, profileImageUrl = u.ProfileImageUrl }).ToListAsync();
                return Ok(users);
            }
            catch (UnauthorizedAccessException ex) { return Error(ex, 401); }
            catch (Exception ex) { return Error(ex); }
        }

        [HttpGet("requests")]
        public async Task<IActionResult> Requests()
        {
            try
            {
                var userId = UserId();
                var requests = await (from f in _context.Friendships
                    join u in _context.Users on f.SenderId equals u.Id
                    where f.ReceiverId == userId && f.Status == FriendshipStatus.Pending
                    orderby f.CreatedAt descending
                    select new { f.Id, userId = u.Id, fullName = u.FullName, u.Email, profileImageUrl = u.ProfileImageUrl }).ToListAsync();
                return Ok(requests);
            }
            catch (UnauthorizedAccessException ex) { return Error(ex, 401); }
            catch (Exception ex) { return Error(ex); }
        }

        [HttpPost("requests")]
        public async Task<IActionResult> SendRequest(FriendshipRequestDto dto)
        {
            try
            {
                var userId = UserId();
                if (dto.UserId == userId) return Error(new ArgumentException("Não é possível adicionar a própria conta."));
                if (!await _context.Users.AnyAsync(u => u.Id == dto.UserId)) return NotFound(new { error = "Perfil não encontrado." });
                var exists = await _context.Friendships.AnyAsync(f => (f.SenderId == userId && f.ReceiverId == dto.UserId) || (f.SenderId == dto.UserId && f.ReceiverId == userId));
                if (exists) return Error(new InvalidOperationException("Já existe uma relação ou pedido entre estes perfis."));
                _context.Friendships.Add(new Friendship(userId, dto.UserId));
                await _context.SaveChangesAsync();
                return StatusCode(201, new { message = "Pedido de amizade enviado." });
            }
            catch (UnauthorizedAccessException ex) { return Error(ex, 401); }
            catch (Exception ex) { return Error(ex); }
        }

        [HttpPost("requests/{id:int}/accept")]
        public Task<IActionResult> Accept(int id) => Respond(id, true);
        [HttpPost("requests/{id:int}/decline")]
        public Task<IActionResult> Decline(int id) => Respond(id, false);

        [HttpDelete("{friendUserId:int}")]
        public async Task<IActionResult> Remove(int friendUserId)
        {
            try
            {
                var userId = UserId();
                var friendship = await _context.Friendships.FirstOrDefaultAsync(f => f.Status == FriendshipStatus.Accepted &&
                    ((f.SenderId == userId && f.ReceiverId == friendUserId) || (f.SenderId == friendUserId && f.ReceiverId == userId)));
                if (friendship == null) return NotFound(new { error = "Amizade não encontrada." });
                _context.Friendships.Remove(friendship);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (UnauthorizedAccessException ex) { return Error(ex, 401); }
            catch (Exception ex) { return Error(ex); }
        }

        private async Task<IActionResult> Respond(int id, bool accept)
        {
            try
            {
                var request = await _context.Friendships.FirstOrDefaultAsync(f => f.Id == id && f.ReceiverId == UserId() && f.Status == FriendshipStatus.Pending);
                if (request == null) return NotFound(new { error = "Pedido não encontrado." });
                if (accept) request.Accept(); else request.Decline();
                await _context.SaveChangesAsync();
                return Ok(new { message = accept ? "Pedido aceito." : "Pedido recusado." });
            }
            catch (UnauthorizedAccessException ex) { return Error(ex, 401); }
            catch (Exception ex) { return Error(ex); }
        }
    }
}
