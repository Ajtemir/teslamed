using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeslaMed.Tests
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using NSubstitute;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using TeslaMed.Controllers;
    using TeslaMed.Models.Repositories;
    using TeslaMed.Models;
    using Xunit;

    public class HistoryControllerTests
    {
        private readonly Mock<IRepository> _mockRepo;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly HistoryController _controller;

        public HistoryControllerTests()
        {
            _mockRepo = new Mock<IRepository>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockUserManager = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _controller = new HistoryController(null, null, _mockUserManager.Object, _mockRepo.Object, _mockEnvironment.Object);
        }

        [Fact]
        public void Index_ReturnsAViewResult_WithAListOfHistories()
        {
            // Arrange
            var mockHistories = new List<History>
        {
            new History { Id = 1, Text = "Test1", Image = "image1.jpg" },
            new History { Id = 2, Text = "Test2", Image = "image2.jpg" }
        };
            _mockRepo.Setup(repo => repo.GetHistories()).Returns(mockHistories);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<History>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public void CreateHistory_Get_ReturnsAViewResult()
        {
            // Arrange

            // Act
            var result = _controller.CreateHistory();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreateHistory_Post_RedirectsToIndexWhenSuccessful()
        {
            // Arrange
            var history = new History { Text = "Test", Image = "image.jpg" };
            var fileMock = new Mock<IFormFile>();
            _mockRepo.Setup(repo => repo.DbAdd(It.IsAny<History>())).Returns(Task.CompletedTask);
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateHistory(history, fileMock.Object);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task EditHistory_Get_ReturnsNotFoundWhenNoHistory()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetHistory(It.IsAny<int>())).Returns((History)null);

            // Act
            var result = await _controller.EditHistory(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditHistory_Post_RedirectsToIndexWhenSuccessful()
        {
            // Arrange
            var history = new History { Id = 1, Text = "Test", Image = "image.jpg" };
            var fileMock = new Mock<IFormFile>();
            _mockRepo.Setup(repo => repo.GetHistories()).Returns(new List<History> { history });
            _mockRepo.Setup(repo => repo.DbUpdate(It.IsAny<History>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.EditHistory(history, fileMock.Object, true);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public async Task RemoveHistory_ReturnsRedirectToActionResult_WhenHistoryIsFound()
        {
            // Arrange
            var historyId = 1;
            var history = new History { Id = historyId, Text = "Test", Image = "image.jpg" };
            _mockRepo.Setup(repo => repo.GetHistories()).Returns(new List<History> { history });
            _mockRepo.Setup(repo => repo.DbRemove(It.IsAny<History>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);
            _mockRepo.Setup(repo => repo.GetHistories()).Returns(new List<History> { new History { Id = 1, Text = "Test1", Image = "image1.jpg" },
            new History { Id = 2, Text = "Test2", Image = "image2.jpg" } });
            // Act
            var result = await _controller.RemoveHistory(historyId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }


    }

}
