using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CareFlow.Infrastructure.Persistence;

public class DesignTimeFactory : IDesignTimeDbContextFactory<CareFlowDbContext>
{
    public CareFlowDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<CareFlowDbContext>()
            .UseSqlite(CareFlowDatabase.DefaultConnectionString)
            .Options;
        return new CareFlowDbContext(options);
    }
}