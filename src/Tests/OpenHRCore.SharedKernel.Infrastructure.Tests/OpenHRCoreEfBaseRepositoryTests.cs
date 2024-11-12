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
        public int Value { get; set; }
        public RelatedEntity? RelatedEntity { get; set; }
        public Guid? RelatedEntityId { get; set; }
    }

    public class RelatedEntity : OpenHRCoreBaseEntity
    {
        public string RelatedName { get; set; } = string.Empty;
    }

    public class OpenHRCoreEfBaseRepositoryTests : IDisposable
    {
        private readonly DbContextOptions<TestDbContext> _options;
        private readonly TestDbContext _context;
        private readonly OpenHRCoreEfBaseRepository<TestEntity> _repository;

        public OpenHRCoreEfBaseRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new TestDbContext(_options);
            _repository = new OpenHRCoreEfBaseRepository<TestEntity>(_context);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_Should_Throw_ArgumentNullException_When_DbContext_Is_Null()
        {
            Assert.Throws<ArgumentNullException>(() => new OpenHRCoreEfBaseRepository<TestEntity>(null!));
        }

        #endregion

        #region Create Operation Tests

        [Fact]
        public void Add_Should_Add_Entity_To_Context()
        {
            // Arrange
            var entity = new TestEntity { TestName = "Test" };

            // Act
            _repository.Add(entity);
            _context.SaveChanges();

            // Assert
            Assert.Single(_context.TestEntities);
        }

        [Fact]
        public void AddRange_Should_Add_Multiple_Entities()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { TestName = "Test1" },
                new TestEntity { TestName = "Test2" }
            };

            // Act
            _repository.AddRange(entities);
            _context.SaveChanges();

            // Assert
            Assert.Equal(2, _context.TestEntities.Count());
        }

        [Fact]
        public async Task AddAsync_Should_Add_Entity_Asynchronously()
        {
            // Arrange
            var entity = new TestEntity { TestName = "Test" };

            // Act
            await _repository.AddAsync(entity);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Single(await _context.TestEntities.ToListAsync());
        }

        [Fact]
        public async Task AddRangeAsync_Should_Add_Multiple_Entities_Asynchronously()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { TestName = "Test1" },
                new TestEntity { TestName = "Test2" }
            };

            // Act
            await _repository.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(2, await _context.TestEntities.CountAsync());
        }

        #endregion

        #region Read Operation Tests

        [Fact]
        public async Task GetAllAsync_Should_Return_All_Entities()
        {
            // Arrange
            await SeedTestData();

            // Act
            var results = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(3, results.Count());
        }

        [Fact]
        public async Task GetAllAsync_With_Includes_Should_Return_All_Entities_With_Related_Data()
        {
            // Arrange
            await SeedTestDataWithRelations();

            // Act
            var results = await _repository.GetAllAsync(e => e.RelatedEntity!);

            // Assert
            Assert.All(results, entity => Assert.NotNull(entity.RelatedEntity));
        }

        [Fact]
        public async Task GetByIdAsync_Should_Return_Entity_When_Exists()
        {
            // Arrange
            var entity = new TestEntity { TestName = "Test" };
            _repository.Add(entity);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(entity.Id);

            // Assert
            Assert.Equal(entity.Id, result!.Id);
        }

        [Fact]
        public async Task GetFirstOrDefaultAsync_Should_Return_Entity_When_Matches_Predicate()
        {
            // Arrange
            await SeedTestData();

            // Act
            var result = await _repository.GetFirstOrDefaultAsync(e => e.TestName == "Test1");

            // Assert
            Assert.Equal("Test1", result!.TestName);
        }

        [Fact]
        public async Task GetPageAsync_Should_Return_Correct_Page()
        {
            // Arrange
            await SeedTestData();

            // Act
            var results = await _repository.GetPageAsync(1, 2);

            // Assert
            Assert.Equal(2, results.Count());
        }

        [Fact]
        public async Task GetPagedAsync_Should_Return_Correct_Count_And_Items()
        {
            // Arrange
            await SeedTestData();
            var searchCriteria = new Dictionary<string, string>
            {
                { "TestName", "Test" }
            };

            // Act
            var result = await _repository.GetPagedAsync(
                pageNumber: 1,
                pageSize: 2,
                orderBy: e => e.TestName,
                ascending: true,
                searchCriteria: searchCriteria,
                e => e.RelatedEntity!
            );

            // Assert
            Assert.Equal(3, result.TotalCount); // Total count should be 3 since all items match "Test" in TestName
            Assert.Equal(2, result.Items.Count()); // Page size is 2
            Assert.True(result.Items.First().TestName.CompareTo(result.Items.Last().TestName) <= 0); // Verify ascending order
        }

        #endregion

        #region Update Operation Tests

        [Fact]
        public void Update_Should_Update_Entity()
        {
            // Arrange
            var entity = new TestEntity { TestName = "Test" };
            _repository.Add(entity);
            _context.SaveChanges();

            // Act
            entity.TestName = "Updated";
            _repository.Update(entity);
            _context.SaveChanges();

            // Assert
            var updated = _context.TestEntities.Find(entity.Id);
            Assert.Equal("Updated", updated!.TestName);
        }

        [Fact]
        public void UpdateRange_Should_Update_Multiple_Entities()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { TestName = "Test1" },
                new TestEntity { TestName = "Test2" }
            };
            _repository.AddRange(entities);
            _context.SaveChanges();

            // Act
            foreach (var entity in entities)
            {
                entity.TestName = "Updated";
            }
            _repository.UpdateRange(entities);
            _context.SaveChanges();

            // Assert
            Assert.All(_context.TestEntities, e => Assert.Equal("Updated", e.TestName));
        }

        #endregion

        #region Delete Operation Tests

        [Fact]
        public void Remove_Should_Remove_Entity()
        {
            // Arrange
            var entity = new TestEntity { TestName = "Test" };
            _repository.Add(entity);
            _context.SaveChanges();

            // Act
            _repository.Remove(entity);
            _context.SaveChanges();

            // Assert
            Assert.Empty(_context.TestEntities);
        }

        [Fact]
        public void RemoveRange_Should_Remove_Multiple_Entities()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { TestName = "Test1" },
                new TestEntity { TestName = "Test2" }
            };
            _repository.AddRange(entities);
            _context.SaveChanges();

            // Act
            _repository.RemoveRange(entities);
            _context.SaveChanges();

            // Assert
            Assert.Empty(_context.TestEntities);
        }

        #endregion

        #region Aggregate Operation Tests

        [Fact]
        public void Max_Should_Return_Maximum_Value()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { TestName = "Test1", Value = 1 },
                new TestEntity { TestName = "Test2", Value = 2 },
                new TestEntity { TestName = "Test3", Value = 3 }
            };
            _repository.AddRange(entities);
            _context.SaveChanges();

            // Act
            var max = _repository.Max(e => e.Value);

            // Assert
            Assert.Equal(3, max);
        }

        [Fact]
        public async Task MaxAsync_Should_Return_Maximum_Value()
        {
            // Arrange
            var entities = new List<TestEntity>
            {
                new TestEntity { TestName = "Test1", Value = 1 },
                new TestEntity { TestName = "Test2", Value = 2 },
                new TestEntity { TestName = "Test3", Value = 3 }
            };
            await _repository.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Act
            var max = await _repository.MaxAsync(e => e.Value);

            // Assert
            Assert.Equal(3, max);
        }

        #endregion

        #region Helper Methods

        private async Task SeedTestData()
        {
            var entities = new List<TestEntity>
            {
                new TestEntity { TestName = "Test1" },
                new TestEntity { TestName = "Test2" },
                new TestEntity { TestName = "Test3" }
            };
            await _repository.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        private async Task SeedTestDataWithRelations()
        {
            var entities = new List<TestEntity>
            {
                new TestEntity { TestName = "Test1", RelatedEntity = new RelatedEntity() },
                new TestEntity { TestName = "Test2", RelatedEntity = new RelatedEntity() },
                new TestEntity { TestName = "Test3", RelatedEntity = new RelatedEntity() }
            };
            await _repository.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        #endregion
    }
}