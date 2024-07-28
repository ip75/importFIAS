using Microsoft.EntityFrameworkCore;

namespace Address.API.Model
{
    public partial class fiasContext : DbContext
    {
        public virtual DbSet<Settings> Settings { get; set; }
    }

    public class Settings
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}