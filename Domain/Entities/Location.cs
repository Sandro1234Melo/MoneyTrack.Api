using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyTrack.Api.Domain.Entities
{
    public class Location
    {
        public int Id { get; private set; }
        public string Name { get; private set; } = string.Empty;

        public int UserId { get; private set; }
        public User? User { get; private set; }

        public ICollection<ShoppingList> ShoppingLists { get; private set; } = new List<ShoppingList>();

        public Location(string name, int userId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Nome do local é obrigatório");

            Name = name;
            UserId = userId;
        }

        public void Update(string name, int userId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Nome inválido");

            Name = name;
            UserId = userId;
        }

        private Location() { }
    }
}
