using Microsoft.AspNetCore.Mvc;
using Moq;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TeslaMed.Controllers;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;
using Xunit;

namespace TeslaMed.Tests
{
    public class PrivilegesControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResult_WithListOfPrivileges()
        {
            // Arrange
            var mockRepo = new Mock<IRepository>();
            mockRepo.Setup(repo => repo.GetAllPrivileges()).Returns(GetTestPrivileges());
            var controller = new PrivilegesController(null, null, null, mockRepo.Object, null);

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Privileges>>(viewResult.Model);
            int count = GetTestPrivileges().Count();
            Assert.Equal(count, model.Count());
        }

        [Fact]
        public void Create_ReturnsViewResult_WhenModelStateIsInvalid()
        {
            // Arrange
            var mockRepo = new Mock<IRepository>();
            var controller = new PrivilegesController(null, null, null, mockRepo.Object, null);
            controller.ModelState.AddModelError("error", "some error");

            // Act
            var result = controller.Create(new Privileges()).Result;

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_ReturnsRedirectToActionResult_WhenModelStateIsValid()
        {
            // Arrange
            var mockRepo = new Mock<IRepository>();
            var controller = new PrivilegesController(null, null, null, mockRepo.Object, null);
            var privileges = new Privileges { Id = 1, Discount = 10, Category = "TestCategory", Description = "TestDescription" };

            // Act
            var result = await controller.Create(privileges);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            mockRepo.Verify(r => r.DbAdd(privileges), Times.Once);
            mockRepo.Verify(r => r.DbSave(), Times.Once);
        }

        [Fact]
        public async Task Edit_ReturnsViewResult_WithPrivileges()
        {
            // Arrange
            var mockRepo = new Mock<IRepository>();
            var controller = new PrivilegesController(null, null, null, mockRepo.Object, null);
            var privileges = new Privileges { Id = 1, Discount = 10, Category = "TestCategory", Description = "TestDescription" };
            mockRepo.Setup(repo => repo.GetPrivileges(1)).Returns(privileges);

            // Act
            var result = await controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Privileges>(viewResult.Model);
            Assert.Equal(privileges, model);
        }


        [Fact]
        public async Task Edit_ReturnsRedirectToActionResult_WhenModelStateIsValid()
        {
            // Arrange
            var mockRepo = new Mock<IRepository>();
            var controller = new PrivilegesController(null, null, null, mockRepo.Object, null);
            var privileges = new Privileges { Id = 1, Discount = 10, Category = "TestCategory", Description = "TestDescription" };

            // Act
            var result = await controller.Edit(1, privileges);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            mockRepo.Verify(r => r.DbUpdate(privileges), Times.Once);
            mockRepo.Verify(r => r.DbSave(), Times.Once);
        }

        [Fact]
        public async Task Remove_ReturnsRedirectToActionResult()
        {
            // Arrange
            var mockRepo = new Mock<IRepository>();
            var controller = new PrivilegesController(null, null, null, mockRepo.Object, null);
            // Mock repository behavior
            var policyId = 1;
            var policy = new Privileges { Id = policyId, Discount = 15, Category = "text", Description = "text" };
            mockRepo.Setup(repo => repo.GetAllPrivileges()).Returns(new List<Privileges> { policy });
            mockRepo.Setup(repo => repo.DbRemove(It.IsAny<Privileges>())).Verifiable();
            mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);
            // Act
            var result = await controller.Remove(1);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            mockRepo.Verify(r => r.DbRemove(It.IsAny<Privileges>()), Times.Once);
            mockRepo.Verify(r => r.DbSave(), Times.Once);
        }



        private List<Privileges> GetTestPrivileges()
        {
            return new List<Privileges>
            {
                new Privileges { Id = 1, Discount = 10, Category = "TestCategory1", Description = "TestDescription1" },
                new Privileges { Id = 2, Discount = 15, Category = "TestCategory2", Description = "TestDescription2" },
            };
        }
    }
}
