using Microsoft.EntityFrameworkCore;

namespace RabbitMq.ExcelCreate.Models
{
    public class DataContext :DbContext
    {
        public DataContext(DbContextOptions<DataContext> options):base(options)
        {
            
        }

        public DbSet<UserFile> UserFiles { get; set; }



    }
}
