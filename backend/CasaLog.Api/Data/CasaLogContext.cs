namespace CasaLog.Api.Data;

public class CasaLogContext(DbContextOptions<CasaLogContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Home> Homes => Set<Home>();
    public DbSet<Equipment> Equipment => Set<Equipment>();
    public DbSet<ScheduledTask> ScheduledTasks => Set<ScheduledTask>();
    public DbSet<TaskCompletion> TaskCompletions => Set<TaskCompletion>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasOne(x => x.Home).WithOne(x => x.User).HasForeignKey<Home>(x => x.UserId);
        });

        modelBuilder.Entity<Home>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.Equipment).WithOne(x => x.Home).HasForeignKey(x => x.HomeId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.ScheduledTasks).WithOne(x => x.Home).HasForeignKey(x => x.HomeId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Alerts).WithOne(x => x.Home).HasForeignKey(x => x.HomeId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Equipment>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasMany(x => x.ScheduledTasks).WithOne(x => x.Equipment).HasForeignKey(x => x.EquipmentId).IsRequired(false).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ScheduledTask>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Completion).WithOne(x => x.Task).HasForeignKey<TaskCompletion>(x => x.TaskId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Alerts).WithOne(x => x.Task).HasForeignKey(x => x.TaskId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<TaskCompletion>(e => e.HasKey(x => x.Id));
        modelBuilder.Entity<Alert>(e => e.HasKey(x => x.Id));
    }
}
