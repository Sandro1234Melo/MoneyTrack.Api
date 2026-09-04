using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoneyTrack.Api.Application.Dtos.Users;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Infrastructure.Data;

namespace MoneyTrack.Api.Presentation.Controllers
{
    [ApiController, Route("api/splits")]
    public class SplitsController : ControllerBase
    {
        private readonly MoneyTrackDbContext _context;
        public SplitsController(MoneyTrackDbContext context) => _context = context;
        private int UserId() => Request.Headers.TryGetValue("X-User-Id", out var v) && int.TryParse(v, out var id) ? id : throw new UnauthorizedAccessException("Usuário não autenticado.");
        private IActionResult Error(Exception ex, int status = 400) => StatusCode(status, new { error = ex.Message });

        [HttpGet]
        public async Task<IActionResult> List()
        {
            try
            {
                var userId = UserId();
                var splits = await _context.ExpenseSplits.Include(s => s.Participants).Where(s => s.Participants.Any(p => p.UserId == userId)).OrderByDescending(s => s.CreatedAt).ToListAsync();
                var ids = splits.SelectMany(s => s.Participants).Select(p => p.UserId).Distinct().ToList();
                var names = await _context.Users.Where(u => ids.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FullName);
                return Ok(splits.Select(s => new { s.Id, s.Description, s.TotalAmount, s.PaidByUserId, s.CreatedAt, participants = s.Participants.Select(p => new { p.Id, p.UserId, name = names.GetValueOrDefault(p.UserId, "Usuário"), p.Amount, p.IsPaid, p.PaidAt }) }));
            }
            catch (UnauthorizedAccessException ex) { return Error(ex, 401); } catch (Exception ex) { return Error(ex); }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExpenseSplitCreateDto dto)
        {
            try
            {
                var userId = UserId();
                if (string.IsNullOrWhiteSpace(dto.Description) || dto.Participants.Count < 2) return Error(new ArgumentException("Informe uma descrição e ao menos duas pessoas."));
                if (dto.Participants.Select(p => p.UserId).Distinct().Count() != dto.Participants.Count || !dto.Participants.Any(p => p.UserId == userId)) return Error(new ArgumentException("Inclua você uma única vez na divisão."));
                if (dto.Participants.Any(p => p.Amount < 0) || Math.Round(dto.Participants.Sum(p => p.Amount), 2) != Math.Round(dto.TotalAmount, 2)) return Error(new ArgumentException("A soma das partes deve ser igual ao valor total."));
                var otherIds = dto.Participants.Where(p => p.UserId != userId).Select(p => p.UserId).ToList();
                var friends = await _context.Friendships.Where(f => f.Status == FriendshipStatus.Accepted && ((f.SenderId == userId && otherIds.Contains(f.ReceiverId)) || (f.ReceiverId == userId && otherIds.Contains(f.SenderId)))).ToListAsync();
                if (friends.Count != otherIds.Count) return Error(new ArgumentException("Só é possível dividir com amigos aceitos."));
                var split = new ExpenseSplit(userId, userId, dto.Description, dto.TotalAmount);
                foreach (var item in dto.Participants) { var participant = new SplitParticipant(item.UserId, item.Amount); if (item.UserId == userId) participant.MarkPaid(); split.AddParticipant(participant); }
                _context.ExpenseSplits.Add(split); await _context.SaveChangesAsync(); return StatusCode(201, new { split.Id });
            }
            catch (UnauthorizedAccessException ex) { return Error(ex, 401); } catch (Exception ex) { return Error(ex); }
        }

        [HttpPost("participants/{id:int}/paid")]
        public async Task<IActionResult> MarkPaid(int id)
        {
            try
            {
                var participant = await _context.SplitParticipants.FirstOrDefaultAsync(p => p.Id == id && p.UserId == UserId());
                if (participant == null) return NotFound(new { error = "Participação não encontrada." }); participant.MarkPaid(); await _context.SaveChangesAsync(); return Ok();
            }
            catch (UnauthorizedAccessException ex) { return Error(ex, 401); } catch (Exception ex) { return Error(ex); }
        }
    }
}
