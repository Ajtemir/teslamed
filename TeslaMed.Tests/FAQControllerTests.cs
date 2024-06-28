using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaMed.Controllers;
using TeslaMed.Models.Repositories;
using TeslaMed.Models;
using Xunit;

namespace TeslaMed.Tests
{
    public class FAQControllerTests
    {
        private readonly Mock<IRepository> _mockRepo;
        private readonly FAQController _controller;

        public FAQControllerTests()
        {
            _mockRepo = new Mock<IRepository>();
            _controller = new FAQController(null, null, null, _mockRepo.Object, null);
        }

        [Fact]
        public void Index_ReturnsViewResult_WithListOfFAQs()
        {
            // Arrange
            var mockFAQs = new List<FAQ>
        {
            new FAQ { Id = 1, Title = "FAQ1", Text = "Answer1" },
            new FAQ { Id = 2, Title = "FAQ2", Text = "Answer2" }
        };
            _mockRepo.Setup(repo => repo.GetAllFAQ()).Returns(mockFAQs);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<FAQ>>(viewResult.ViewData.Model);
            Assert.Equal(mockFAQs.Count, model.Count());
        }

        [Fact]
        public void CreateFAQ_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.CreateFAQ();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreateFAQ_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var faq = new FAQ { Id = 3, Title = "FAQ3", Text = "Answer3" };
            _mockRepo.Setup(repo => repo.DbAdd(It.IsAny<FAQ>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _controller.CreateFAQ(faq);

            // Assert
            _mockRepo.Verify(repo => repo.DbAdd(It.IsAny<FAQ>()), Times.Once);
            _mockRepo.Verify(repo => repo.DbSave(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("FAQ", redirectToActionResult.ControllerName);
        }

        [Fact]
        public async Task EditFAQ_Get_ReturnsNotFound_WhenFAQDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetFAQ(It.IsAny<int>())).Returns((FAQ)null);

            // Act
            var result = await _controller.EditFAQ(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditFAQ_Get_ReturnsViewResult_WithFAQ()
        {
            // Arrange
            var faq = new FAQ { Id = 1, Title = "FAQ1", Text = "Answer1" };
            _mockRepo.Setup(repo => repo.GetFAQ(1)).Returns(faq);

            // Act
            var result = await _controller.EditFAQ(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<FAQ>(viewResult.ViewData.Model);
            Assert.Equal(faq, model);
        }

        [Fact]
        public async Task EditFAQ_Post_RedirectsToIndex_WhenSuccessful()
        {
            // Arrange
            var faq = new FAQ { Id = 1, Title = "FAQ1", Text = "Answer1" };
            _mockRepo.Setup(repo => repo.GetFAQ(faq.Id)).Returns(faq);
            _mockRepo.Setup(repo => repo.DbUpdate(It.IsAny<FAQ>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _controller.EditFAQ(faq.Id, faq);

            // Assert
            _mockRepo.Verify(repo => repo.DbUpdate(It.IsAny<FAQ>()), Times.Once);
            _mockRepo.Verify(repo => repo.DbSave(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("FAQ", redirectToActionResult.ControllerName);
        }

        [Fact]
        public async Task RemoveFAQ_ReturnsNotFound_WhenFAQDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetAllFAQ()).Returns(new List<FAQ>());

            // Act
            var result = await _controller.RemoveFAQ(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RemoveFAQ_RedirectsToIndex_WhenSuccessful()
        {
            // Arrange
            var faq = new FAQ { Id = 1, Title = "FAQ1", Text = "Answer1" };
            _mockRepo.Setup(repo => repo.GetAllFAQ()).Returns(new List<FAQ> { faq });
            _mockRepo.Setup(repo => repo.DbRemove(It.IsAny<FAQ>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _controller.RemoveFAQ(1);

            // Assert
            _mockRepo.Verify(repo => repo.DbRemove(It.IsAny<FAQ>()), Times.Once);
            _mockRepo.Verify(repo => repo.DbSave(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("FAQ", redirectToActionResult.ControllerName);
        }
    }

}
