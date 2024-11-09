using Microsoft.EntityFrameworkCore;
using OpenHRCore.SharedKernel.Domain;
using static OpenHRCore.SharedKernel.Infrastructure.Tests.OpenHRCoreEfBaseRepositoryTests;

namespace OpenHRCore.SharedKernel.Infrastructure.Tests
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TestEntity> TestEntities { get; set; }
        public DbSet<RelatedEntity> RelatedEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TestEntity>();
            modelBuilder.Entity<RelatedEntity>();
            base.OnModelCreating(modelBuilder);
        }
    }
    public class TestEntity : OpenHRCoreBaseEntity
    {
        public required string TestName { get; set; }
        public RelatedEntity? RelatedEntity { get; set; }
        public Guid? RelatedEntityId { get; set; }
    }

    public class RelatedEntity : OpenHRCoreBaseEntity
    {
        public string RelatedName { get; set; }
    }

    public class OpenHRCoreEfBaseRepositoryTests
    {
        private readonly DbContextOptions<TestDbContext> _options;

        public OpenHRCoreEfBaseRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: "OpenHRCoreTestDataBase")
                .Options;
        }

        [Fact]
        public async Task AddAsync_Should_Add_Entity()
        {
            using var context = new TestDbContext(_options);
            var repository = new OpenHRCoreEfBaseRepository<TestEntity>(context);

            var entity = new TestEntity { TestName = "Test_Value_1" };

            await repository.AddAsync(entity);
            await context.SaveChangesAsync();

            var result = await repository.GetByIdAsync(entity.Id);

            Assert.Equal("Test_Value_1", result.TestName);
        }

        [Fact]
        public async Task GetFirstOrDefaultAsync_WithInclude_Should_Return_EntityWithRelatedData()
        {
            // Arrange
            using var context = new TestDbContext(_options);
            var repository = new OpenHRCoreEfBaseRepository<TestEntity>(context);

            var relatedEntity = new RelatedEntity { RelatedName = "Related_1" };
            await context.RelatedEntities.AddAsync(relatedEntity);
            await context.SaveChangesAsync();

            var entity = new TestEntity 
            { 
                TestName = "Test_1",
                RelatedEntityId = relatedEntity.Id
            };
            await context.TestEntities.AddAsync(entity);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.GetFirstOrDefaultAsync(
                e => e.TestName == "Test_1",
                e => e.RelatedEntity
            );

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.RelatedEntity);
            Assert.Equal("Related_1", result.RelatedEntity.RelatedName);
        }
    }

}