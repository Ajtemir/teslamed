using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeslaMed.Tests
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Localization;
    using TeslaMed.Models;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Moq;
    using Xunit;
    using Microsoft.AspNetCore.Hosting;
    using NSubstitute;
    using TeslaMed.Controllers;
    using TeslaMed.Models.Repositories;
    using Microsoft.AspNetCore.Http;

    public class LicensesControllerTests
    {
        private readonly Mock<IRepository> _repoMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<IStringLocalizer<LicensesController>> _localizerMock;
        private readonly Mock<IWebHostEnvironment> _environmentMock;
        private readonly LicensesController _controller;

        public LicensesControllerTests()
        {
            _repoMock = new Mock<IRepository>();
            _userManagerMock = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _localizerMock = new Mock<IStringLocalizer<LicensesController>>();
            _environmentMock = new Mock<IWebHostEnvironment>();

            _controller = new LicensesController(null, _localizerMock.Object, _userManagerMock.Object, _repoMock.Object, _environmentMock.Object);
        }

        [Fact]
        public void Index_ReturnsViewResult_WithListOfLicenses()
        {
            // Arrange
            var licenses = new List<Licences> { new Licences(), new Licences() };
            _repoMock.Setup(repo => repo.GetLicences()).Returns(licenses);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Licences>>(viewResult.ViewData.Model);
            Assert.Equal(2, ((List<Licences>)model).Count);
        }

        [Fact]
        public void Create_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var license = new Licences
            {
                Id = 1, 
                Photos = new List<string> { "/images/photo1.jpg", "/images/photo2.jpg" } 
            };
            var photoMock = new Mock<IFormFile>();
            var content = "FAKE IMAGE CONTENT"; 
            var fileName = "test.jpg"; 
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            photoMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            photoMock.Setup(_ => _.FileName).Returns(fileName);
            photoMock.Setup(_ => _.Length).Returns(ms.Length);
            var photos = new List<IFormFile> { photoMock.Object };

            _repoMock.Setup(repo => repo.DbAdd(It.IsAny<Licences>())).Returns(Task.CompletedTask);
            _repoMock.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Create(license, photos);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }


        [Fact]
        public async Task Remove_ValidId_RedirectsToIndex()
        {
            // Arrange
            var licenseId = 1;
            var license = new Licences { Id = licenseId };
            _repoMock.Setup(repo => repo.GetLicences()).Returns(new List<Licences> { license });
            _repoMock.Setup(repo => repo.DbRemove(It.IsAny<Licences>())).Verifiable();
            _repoMock.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Remove(licenseId);

            // Assert
            _repoMock.Verify(repo => repo.DbRemove(It.IsAny<Licences>()), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        [Fact]
        public void DeletePhoto_ValidPhoto_RemovesPhotoAndReturnsJsonTrue()
        {
            // Arrange
            var licenseId = 1;
            var photoPath = "/images/photo.jpg";
            var license = new Licences { Id = licenseId, Photos = new List<string> { photoPath, "/images/photo2.jpg" } }; // Добавлена вторая фотография
            _repoMock.Setup(repo => repo.GetLicences()).Returns(new List<Licences> { license });
            _repoMock.Setup(repo => repo.DbUpdate(It.IsAny<Licences>())).Verifiable();
            _repoMock.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);

            // Act
            var result = _controller.DeletePhoto(photoPath, licenseId);

            // Assert
            _repoMock.Verify(repo => repo.DbUpdate(It.IsAny<Licences>()), Times.Once);
            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.True((bool)jsonResult.Value);
        }


    }

}
