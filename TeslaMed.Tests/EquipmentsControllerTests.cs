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
    public class EquipmentsControllerTests
    {
        private readonly Mock<IRepository> _mockRepo;
        private readonly EquipmentsController _controller;

        public EquipmentsControllerTests()
        {
            _mockRepo = new Mock<IRepository>();
            _controller = new EquipmentsController(null, null, null, _mockRepo.Object);
        }

        [Fact]
        public void Index_ReturnsViewResult_WithListOfEquipments()
        {
            // Arrange
            var mockEquipments = new List<Equipment>
        {
            new Equipment { Id = 1, Name = "Equipment1", Text = "Description1", Image = "image1.png" },
            new Equipment { Id = 2, Name = "Equipment2", Text = "Description2", Image = "image2.png" }
        };
            _mockRepo.Setup(repo => repo.GetEquipments()).Returns(mockEquipments);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Equipment>>(viewResult.ViewData.Model);
            Assert.Equal(mockEquipments.Count, model.Count());
        }

        [Fact]
        public void CreateEquipment_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.CreateEquipment();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreateEquipment_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var equipment = new Equipment { Id = 3, Name = "Equipment3", Text = "Description3", Image = "image3.png" };
            var fileMock = new Mock<IFormFile>();
            var fileName = "testImage.png";
            var content = "Fake file content";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;
            fileMock.Setup(_ => _.OpenReadStream()).Returns(ms);
            fileMock.Setup(_ => _.FileName).Returns(fileName);
            fileMock.Setup(_ => _.Length).Returns(ms.Length);

            _mockRepo.Setup(repo => repo.DbAdd(It.IsAny<Equipment>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _controller.CreateEquipment(equipment, fileMock.Object);

            // Assert
            _mockRepo.Verify(repo => repo.DbAdd(It.IsAny<Equipment>()), Times.Once);
            _mockRepo.Verify(repo => repo.DbSave(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Equipments", redirectToActionResult.ControllerName);
        }
        // Добавляем в класс EquipmentsControllerTests

        [Fact]
        public async Task EditEquipment_Get_ReturnsNotFound_WhenEquipmentDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetEquipment(It.IsAny<int>())).Returns((Equipment)null);

            // Act
            var result = await _controller.EditEquipment(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditEquipment_Get_ReturnsViewResult_WithEquipment()
        {
            // Arrange
            var equipment = new Equipment { Id = 1, Name = "Equipment1", Text = "Description1", Image = "image1.png" };
            _mockRepo.Setup(repo => repo.GetEquipment(1)).Returns(equipment);

            // Act
            var result = await _controller.EditEquipment(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Equipment>(viewResult.ViewData.Model);
            Assert.Equal(equipment, model);
        }

        [Fact]
        public async Task EditEquipment_Post_ReturnsViewResult_WhenModelIsNull()
        {
            // Act
            var result = await _controller.EditEquipment(null, null, false);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewData.Model);
        }

        [Fact]
        public async Task EditEquipment_Post_ReturnsNotFound_WhenEquipmentDoesNotExist()
        {
            // Arrange
            var equipment = new Equipment { Id = 1, Name = "Equipment1", Text = "Description1", Image = "image1.png" };
            _mockRepo.Setup(repo => repo.GetEquipments()).Returns(new List<Equipment>());

            // Act
            var result = await _controller.EditEquipment(equipment, null, false);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditEquipment_Post_RedirectsToIndex_WhenSuccessful()
        {
            // Arrange
            var equipment = new Equipment { Id = 1, Name = "Equipment1", Text = "Description1", Image = "image1.png" };
            var fileMock = new Mock<IFormFile>();
            _mockRepo.Setup(repo => repo.GetEquipments()).Returns(new List<Equipment> { equipment });
            _mockRepo.Setup(repo => repo.DbUpdate(It.IsAny<Equipment>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _controller.EditEquipment(equipment, fileMock.Object, true);

            // Assert
            _mockRepo.Verify(repo => repo.DbUpdate(It.IsAny<Equipment>()), Times.Once);
            _mockRepo.Verify(repo => repo.DbSave(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Equipments", redirectToActionResult.ControllerName);
        }

        [Fact]
        public async Task RemoveEquipment_ReturnsNotFound_WhenEquipmentDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetEquipments()).Returns(new List<Equipment>());

            // Act
            var result = await _controller.RemoveEquipment(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task RemoveEquipment_RedirectsToIndex_WhenSuccessful()
        {
            // Arrange
            var equipment = new Equipment { Id = 1, Name = "Equipment1", Text = "Description1", Image = "image1.png" };
            _mockRepo.Setup(repo => repo.GetEquipments()).Returns(new List<Equipment> { equipment });
            _mockRepo.Setup(repo => repo.DbRemove(It.IsAny<Equipment>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask).Verifiable();

            // Act
            var result = await _controller.RemoveEquipment(1);

            // Assert
            _mockRepo.Verify(repo => repo.DbRemove(It.IsAny<Equipment>()), Times.Once);
            _mockRepo.Verify(repo => repo.DbSave(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Equipments", redirectToActionResult.ControllerName);
        }

    }

}
