using Microsoft.EntityFrameworkCore;
using MoneyTrack.Api.Application.Interfaces;
using MoneyTrack.Api.Domain.Entities;
using MoneyTrack.Api.Domain.Enum;
using MoneyTrack.Api.Infrastructure.Data;

namespace MoneyTrack.Api.Data.Repositories
{
    public class ShoppingListRepository : IShoppingListRepository
    {
        private readonly MoneyTrackDbContext _context;

        public ShoppingListRepository(MoneyTrackDbContext context)
        {
            _context = context;
        }

        public async Task<List<ShoppingList>> GetByUser(int userId, string? search = null, string? status = null)
        {
            var query = _context.ShoppingLists
                .Include(l => l.Location)
                .Include(l => l.Items)
                    .ThenInclude(i => i.Category)
                .Where(l => l.UserId == userId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(l => l.Name.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(status) && !status.Equals("Todas", StringComparison.OrdinalIgnoreCase))
            {
                var normalized = status.Trim().ToLower();

                if (normalized is "ativas" or "ativa" or "active" or "draft")
                    query = query.Where(l => l.Status == ShoppingListStatusEnum.Draft);
                else if (normalized is "em andamento" or "andamento" or "inprogress" or "in_progress")
                    query = query.Where(l => l.Status == ShoppingListStatusEnum.InProgress);
                else if (normalized is "finalizadas" or "finalizada" or "completed" or "complete")
                    query = query.Where(l => l.Status == ShoppingListStatusEnum.Completed || l.Status == ShoppingListStatusEnum.Converted);
                else if (Enum.TryParse<ShoppingListStatusEnum>(status, true, out var parsed))
                    query = query.Where(l => l.Status == parsed);
            }

            return await query
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<ShoppingList?> GetById(int id)
        {
            return await _context.ShoppingLists
                .Include(l => l.Location)
                .Include(l => l.Items)
                    .ThenInclude(i => i.Category)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<ShoppingList> Add(ShoppingList list)
        {
            _context.ShoppingLists.Add(list);
            await _context.SaveChangesAsync();
            return list;
        }

        public async Task Update(ShoppingList list)
        {
            // A entidade ja vem rastreada pelo DbContext quando e carregada por GetById.
            // Usar Update(list) em um grafo com Include marca Location, Category e Items como Modified
            // e pode gerar erros de banco em relacionamentos/colunas. SaveChanges e suficiente.
            await _context.SaveChangesAsync();
        }

        public async Task Delete(ShoppingList list)
        {
            _context.ShoppingLists.Remove(list);
            await _context.SaveChangesAsync();
        }
    }
}
