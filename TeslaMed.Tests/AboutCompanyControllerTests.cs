using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NSubstitute;
using TeslaMed.Controllers;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;
using TeslaMed.ViewModels;
using Xunit;

namespace TeslaMed.Tests
{
    public class AboutCompanyControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResult_WithListOfAboutCompany()
        {
            // Arrange
            var mock = new Mock<IRepository>();
            mock.Setup(repo => repo.GetAboutCompanies()).Returns(GetTests());
            var controller = new AboutCompanyController(null, null, null, mock.Object);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<AboutCompany>>(viewResult.Model);
            int count = GetTests().Count; 
            Assert.Equal(count, model.Count);
        }

        private List<AboutCompany> GetTests()
        {
            var news = new List<AboutCompany>
            {
                new AboutCompany
                {
                    Id = 1,
                    Text = "babalblablla",
                    Image = "~/images/Logo_TESLAMED2.png",
                },
                new AboutCompany
                {
                    Id = 2,
                    Text = "bababbblblablla",
                    Image = "~/images/Logo_TESLAMED2.png",
                },
            };
            return news;
        }

        [Fact]
        public async Task CreateAbout_ReturnsRedirectToActionResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var mockRepo = new Mock<IRepository>();
            var controller = new AboutCompanyController(null, null, null, mockRepo.Object);
            controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = await controller.CreateAbout(new AboutCompany(), null);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("AboutCompany", redirectToActionResult.ControllerName);
        }

        [Fact]
        public async Task CreateAbout_ReturnsRedirectToActionIndex_WhenModelStateIsValid()
        {
            // Arrange
            var mockRepo = new Mock<IRepository>();
            mockRepo.Setup(repo => repo.DbAdd<AboutCompany>(It.IsAny<AboutCompany>())).Returns(Task.CompletedTask);
            mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);
            var controller = new AboutCompanyController(null, null, null, mockRepo.Object);
            var aboutCompany = new AboutCompany();
            var fileMock = new Mock<IFormFile>();
            var fileName = "test.jpg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write("dummy image data");
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            // Act
            var result = await controller.CreateAbout(aboutCompany, fileMock.Object);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("AboutCompany", redirectToActionResult.ControllerName);
        }

        [Fact]
        public async Task CreateAbout_AddsAboutCompanyToRepository_WhenModelStateIsValid()
        {
            // Arrange
            var mockRepo = new Mock<IRepository>();
            var controller = new AboutCompanyController(null, null, null, mockRepo.Object);
            var aboutCompany = new AboutCompany();
            var fileMock = new Mock<IFormFile>();
            var fileName = "test.jpg";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write("dummy image data");
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            // Act
            await controller.CreateAbout(aboutCompany, fileMock.Object);

            // Assert
            mockRepo.Verify(repo => repo.DbAdd(It.IsAny<AboutCompany>()), Times.Once());
            mockRepo.Verify(repo => repo.DbSave(), Times.Once());
        }

        [Fact]
        public async Task EditAbout_ReturnsNotFound_WhenAboutCompanyIsNull()
        {
            // Arrange
            var mockRepo = new Mock<IRepository>();
            mockRepo.Setup(repo => repo.GetAboutCompany(It.IsAny<int>())).Returns(value: null);
            var controller = new AboutCompanyController(null, null, null, mockRepo.Object);

            // Act
            var result = await controller.EditAbout(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditAbout_ReturnsViewWithAboutCompany_WhenAboutCompanyIsFound()
        {
            // Arrange
            var aboutCompany = new AboutCompany { Id = 1, Text = "Test", Image = "test.jpg" };
            var mockRepo = new Mock<IRepository>();
            mockRepo.Setup(repo => repo.GetAboutCompany(It.IsAny<int>())).Returns(aboutCompany);
            var controller = new AboutCompanyController(null, null, null, mockRepo.Object);

            // Act
            var result = await controller.EditAbout(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<AboutCompany>(viewResult.ViewData.Model);
            Assert.Equal(aboutCompany, model);
        }

        [Fact]
        public async Task EditAbout_UpdatesAboutCompany_WhenModelStateIsValid()
        {
            // Arrange
            var aboutCompany = new AboutCompany { Id = 1, Text = "Test", Image = "test.jpg" };
            var mockRepo = new Mock<IRepository>();
            mockRepo.Setup(repo => repo.GetAboutCompany(It.IsAny<int>())).Returns(aboutCompany);
            mockRepo.Setup(repo => repo.DbUpdate(It.IsAny<AboutCompany>())).Verifiable();
            mockRepo.Setup(repo => repo.DbSave()).Verifiable();
            mockRepo.Setup(repo => repo.GetAboutCompanies()).Returns(new List<AboutCompany> { aboutCompany });

            var controller = new AboutCompanyController(null, null, null, mockRepo.Object);

            // Act
            var result = await controller.EditAbout(aboutCompany, null, false);

            // Assert
            mockRepo.Verify(repo => repo.DbUpdate(It.IsAny<AboutCompany>()), Times.Once());
            mockRepo.Verify(repo => repo.DbSave(), Times.Once());
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("AboutCompany", redirectToActionResult.ControllerName);
        }



        [Fact]
        public async Task RemoveAbout_RemovesAboutCompanyAndRedirectsToIndex()
        {
            var mockRepo = new Mock<IRepository>();
            var controller = new AboutCompanyController(null, null, null, mockRepo.Object);
            var newsToDelete = GetTests().First();
            mockRepo.Setup(repo => repo.GetAboutCompanies()).Returns(GetTests());
            var result = await controller.RemoveAbout(newsToDelete.Id);
            mockRepo.Verify(r => r.DbRemove<AboutCompany>(It.Is<AboutCompany>(n => n.Id == newsToDelete.Id)), Times.Once);
            mockRepo.Verify(r => r.DbSave(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("AboutCompany", redirectToActionResult.ControllerName);
        }

    }
}
