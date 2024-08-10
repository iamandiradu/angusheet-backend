using Microsoft.EntityFrameworkCore;
using TimesheetValidationApi.Models;

namespace TimesheetValidationApi.Data
{
    public class TimesheetDbContext : DbContext
    {
        public TimesheetDbContext(DbContextOptions<TimesheetDbContext> options)
            : base(options)
        {
        }

        public DbSet<TimesheetEntry> TimesheetEntries { get; set; } // Add other DbSets as needed
    }

    public class TimesheetEntry
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public int Duration => (int)(EndDateTime - StartDateTime).TotalMinutes;
        public string TaskFlags { get; set; }
    }
}
