using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RunGroopWebApp.Controllers;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
namespace RunGroopWebApp.Tests.Controller
{
    public class ClubControllerTests
    {
        private readonly Mock<IClubRepository> _clubRepository;
        private readonly Mock<IPhotoService> _photoService;
        private readonly ClubController _clubController;
        public ClubControllerTests()
        {
            //Dependencies
            _clubRepository = new Mock<IClubRepository>();
            _photoService = new Mock<IPhotoService>();
            //SUT --> System under test
            _clubController = new ClubController(_clubRepository.Object, _photoService.Object);
        }

        [Fact]
        public async Task ClubController_Index_ReturnsViewResult_WithExpectedModel()
        {
            //Arrange - What do i need to bring in?
            var clubs = new List<Club>
            {
                new Club { Id = 1, Title = "Test Club A" },
                new Club { Id = 2, Title = "Test Club B" }
            };

            _clubRepository.Setup(r => r.GetSliceAsync(0, 6)).ReturnsAsync(clubs);
            _clubRepository.Setup(r => r.GetCountAsync()).ReturnsAsync(clubs.Count);


            //Act
            var result = await _clubController.Index();

            //Assert - Object check actions and view model
            result.Should().BeOfType<ViewResult>();
            var viewResult = result as ViewResult;
            viewResult!.Model.Should().BeOfType<IndexClubViewModel>();
            var model = viewResult.Model as IndexClubViewModel;

            model!.Clubs.Should().HaveCount(2);
            model.Clubs.First().Title.Should().Be("Test Club A");
            model.Page.Should().Be(1);
            model.PageSize.Should().Be(6);
            model.TotalClubs.Should().Be(2);
            model.TotalPages.Should().Be(1);
            model.Category.Should().Be(-1);
        }

        [Fact]
        public async Task ClubController_Index_ReturnsNotFound_WhenInvalidPageOrPageSize()
        {
            // Act
            var result = await _clubController.Index(page: 0, pageSize: 0);

            //Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
