using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RunGroopWebApp.Data;
using RunGroopWebApp.Data.Enum;
using RunGroopWebApp.Models;
using RunGroopWebApp.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;


namespace RunGroopWebApp.Tests.Repository
{
    public class ClubRepositoryTests
    {
        private readonly ClubRepository _repository;

        private async Task<ApplicationDbContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var databaseContext = new ApplicationDbContext(options);
            databaseContext.Database.EnsureCreated();
            if (await databaseContext.Clubs.CountAsync() <= 0)
            {
                var testClubs = new[]
                {
                    new Club()
                    {
                        Title = "Running Club 1",
                        Image = "https://www.eatthis.com/wp-content/uploads/sites/4/2020/05/running.jpg?quality=82&strip=1&resize=640%2C360",
                        Description = "This is the description of the first club",
                        ClubCategory = ClubCategory.City,
                        Address = new Address()
                        {
                            Street = "123 Main St",
                            City = "Charlotte",
                            State = "NC"
                        }
                    },
                    new Club()
                    {
                        Title = "Trail Running Club",
                        Image = "https://example.com/trail.jpg",
                        Description = "Trail running enthusiasts",
                        ClubCategory = ClubCategory.Trail,
                        Address = new Address()
                        {
                            Street = "456 Trail Rd",
                            City = "Asheville",
                            State = "NC"
                        }
                    },
                    new Club()
                    {
                        Title = "Endurance Running Club",
                        Image = "https://example.com/endurance.jpg",
                        Description = "Long distance endurance running",
                        ClubCategory = ClubCategory.Endurance,
                        Address = new Address()
                        {
                            Street = "789 Speed Ave",
                            City = "Raleigh",
                            State = "NC"
                        }
                    },
                    new Club()
                    {
                        Title = "Club Without Address",
                        Image = "https://example.com/no-address.jpg",
                        Description = "Club with no physical address",
                        ClubCategory = ClubCategory.City,
                        Address = null
                    }
                };

                await databaseContext.Clubs.AddRangeAsync(testClubs);
                await databaseContext.SaveChangesAsync();
            }
            return databaseContext;
        }


        [Fact]
        public async void ClubRepository_Add_ReturnsBool()
        {
            //Arrange
            var club = new Club()
            {
                Title = "Running Club 1",
                Image = "https://www.eatthis.com/wp-content/uploads/sites/4/2020/05/running.jpg?quality=82&strip=1&resize=640%2C360",
                Description = "This is the description of the first cinema",
                ClubCategory = ClubCategory.City,
                Address = new Address()
                {
                    Street = "123 Main St",
                    City = "Charlotte",
                    State = "NC"
                }
            };
            var dbContext = await GetDbContext();
            var clubRepository = new ClubRepository(dbContext);

            //Act
            var result = clubRepository.Add(club);

            //Assert
            result.Should().BeTrue();

        }

        [Fact]
        public async Task GetByIdAsync_WhenClubExists_ShouldReturnClubWithAddress()
        {
            //Arrange
            using var context = await GetDbContext();
            var repository = new ClubRepository(context);
            //Act
            var expectedClub = await context.Clubs.Include(c => c.Address).FirstAsync();
            var expectedId = expectedClub.Id;
            //Assert
            var result = await repository.GetByIdAsync(expectedId);

            result.Should().NotBeNull();
            result.Id.Should().Be(expectedId);
            result.Title.Should().Be("Running Club 1");
            result.Description.Should().Be("This is the description of the first club");
            result.ClubCategory.Should().Be(ClubCategory.City);
            result.Address.Should().NotBeNull();
            result.Address.Street.Should().Be("123 Main St");
            result.Address.City.Should().Be("Charlotte");
            result.Address.State.Should().Be("NC");
        }

        [Fact]
        public async Task GetByIdAsync_WhenClubDoesNotExist_ShouldReturnNull()
        {
            //Arrange
            using var context = await GetDbContext();
            var repository = new ClubRepository(context);
            var nonExistenId = 9999;

            //Act
            var result = await repository.GetByIdAsync(nonExistenId);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetByIdAsync_WhenClubExistsWithoutAddress_ShouldReturnClubWithNullAddress()
        {
            //Arrange
            using var context = await GetDbContext();
            var repository = new ClubRepository(context);

            //Act
            var clubWithoutAddress = await context.Clubs
                .Where(c => c.Address == null)
                .FirstAsync();
            var result = await repository.GetByIdAsync(clubWithoutAddress.Id);

            //Assert
            result.Should().NotBeNull();
            result.Address.Should().BeNull();
            result.Title.Should().Be("Club Without Address");
        }
    }
}
