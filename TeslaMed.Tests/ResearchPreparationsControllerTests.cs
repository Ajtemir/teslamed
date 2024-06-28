using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NSubstitute;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TeslaMed.Controllers;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;
using Xunit;

namespace TeslaMed.Tests
{
    public class ResearchPreparationsControllerTests
    {
        private readonly ResearchPreparationsController _controller;
        private readonly Mock<IRepository> _mockRepo;

        public ResearchPreparationsControllerTests()
        {
            _mockRepo = new Mock<IRepository>();
            _controller = new ResearchPreparationsController(null, null, null, _mockRepo.Object)
            {
                ControllerContext = new ControllerContext()
            };
        }

        [Fact]
        public void Index_ReturnsViewResult_WithAllPreparations()
        {
            // Arrange
            var preparations = new List<ResearchPreparation> { new ResearchPreparation(), new ResearchPreparation() };
            _mockRepo.Setup(repo => repo.GetAllPreparations()).Returns(preparations);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ResearchPreparation>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            // Arrange

            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var preparation = new ResearchPreparation { Title = "Test", Text = "Test text", Image = "test.jpg" };
            var fileMock = new Mock<IFormFile>();
            _mockRepo.Setup(repo => repo.DbAdd<ResearchPreparation>(It.IsAny<ResearchPreparation>())).Returns(Task.CompletedTask);
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(preparation, fileMock.Object);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task Edit_Get_ValidId_ReturnsViewResult_WithPreparation()
        {
            // Arrange
            var preparationId = 1;
            var preparation = new ResearchPreparation { Id = preparationId, Title = "Test", Text = "Test text", Image = "test.jpg" };
            _mockRepo.Setup(repo => repo.GetPreparation(preparationId)).Returns(preparation);

            // Act
            var result = await _controller.Edit(preparationId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ResearchPreparation>(viewResult.ViewData.Model);
            Assert.Equal(preparationId, model.Id);
        }

        [Fact]
        public async Task Edit_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var preparation = new ResearchPreparation { Id = 1, Title = "Updated Test", Text = "Updated test text", Image = "updated_test.jpg" };
            var preparations = new List<ResearchPreparation> { preparation };
            var fileMock = new Mock<IFormFile>();
            _mockRepo.Setup(repo => repo.GetPreparation(preparation.Id)).Returns(preparation);
            _mockRepo.Setup(repo => repo.GetAllPreparations()).Returns(preparations);
            _mockRepo.Setup(repo => repo.DbUpdate(It.IsAny<ResearchPreparation>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(preparation, fileMock.Object, true);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            _mockRepo.Verify();
        }

        [Fact]
        public async Task Remove_ValidId_RedirectsToIndex()
        {
            // Arrange
            var preparationId = 1;
            var preparation = new ResearchPreparation { Id = preparationId, Title = "Test", Text = "Test text", Image = "test.jpg" };
            _mockRepo.Setup(repo => repo.GetAllPreparations()).Returns(new List<ResearchPreparation> { preparation });
            _mockRepo.Setup(repo => repo.DbRemove(It.IsAny<ResearchPreparation>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Remove(preparationId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            _mockRepo.Verify();
        }

        [Fact]
        public async Task Remove_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var preparationId = 1;
            _mockRepo.Setup(repo => repo.GetAllPreparations()).Returns(new List<ResearchPreparation>());

            // Act
            var result = await _controller.Remove(preparationId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
