using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
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
    public class PolicyControllerTests
    {
        private readonly PolicyController _controller;
        private readonly Mock<IRepository> _mockRepo;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IStringLocalizer<PolicyController>> _mockLocalizer;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<ISession> _mockSession;

        public PolicyControllerTests()
        {
            _mockRepo = new Mock<IRepository>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _mockUserManager = new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null, null, null, null, null, null, null, null);
            _mockLocalizer = new Mock<IStringLocalizer<PolicyController>>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext.Setup(s => s.Session).Returns(_mockSession.Object);
            var context = new DefaultHttpContext { Session = _mockSession.Object };
            _controller = new PolicyController(null, _mockLocalizer.Object, _mockUserManager.Object, _mockRepo.Object, _mockEnvironment.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = context
                }
            };
        }

        [Fact]
        public void Index_ReturnsViewResult_WithAllPolicies()
        {
            // Arrange
            var policies = new List<Policy> { new Policy(), new Policy() };
            _mockRepo.Setup(repo => repo.GetAllPolicies()).Returns(policies);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Policy>>(viewResult.ViewData.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public void CreatePolicy_Get_ReturnsViewResult()
        {
            // Arrange

            // Act
            var result = _controller.CreatePolicy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task CreatePolicy_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var policy = new Policy { Text = "Test", Image = "test.jpg" };
            var fileMock = new Mock<IFormFile>();
            _mockRepo.Setup(repo => repo.DbAdd(It.IsAny<Policy>())).Returns(Task.CompletedTask);
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreatePolicy(policy, fileMock.Object);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }
        [Fact]
        public async Task EditPolicy_Get_ValidId_ReturnsViewResult_WithPolicy()
        {
            // Arrange
            var policyId = 1;
            var policy = new Policy { Id = policyId, Text = "Test", Image = "test.jpg" };
            _mockRepo.Setup(repo => repo.GetPolicy(policyId)).Returns(policy);

            // Act
            var result = await _controller.EditPolicy(policyId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Policy>(viewResult.ViewData.Model);
            Assert.Equal(policyId, model.Id);
        }

        [Fact]
        public async Task EditPolicy_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var policy = new Policy { Id = 1, Text = "Updated Test", Image = "updated_test.jpg" };
            var policies = new List<Policy> { policy };
            var fileMock = new Mock<IFormFile>();
            _mockRepo.Setup(repo => repo.GetPolicy(policy.Id)).Returns(policy);
            _mockRepo.Setup(repo => repo.GetAllPolicies()).Returns(policies); // Настройка мока для возврата списка политик
            _mockRepo.Setup(repo => repo.DbUpdate(It.IsAny<Policy>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.EditPolicy(policy, fileMock.Object, true);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            _mockRepo.Verify();
        }


        [Fact]
        public async Task RemovePolicy_ValidId_RedirectsToIndex()
        {
            // Arrange
            var policyId = 1;
            var policy = new Policy { Id = policyId, Text = "Test", Image = "test.jpg" };
            _mockRepo.Setup(repo => repo.GetAllPolicies()).Returns(new List<Policy> { policy });
            _mockRepo.Setup(repo => repo.DbRemove(It.IsAny<Policy>())).Verifiable();
            _mockRepo.Setup(repo => repo.DbSave()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.RemovePolicy(policyId);

            // Assert
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            _mockRepo.Verify();
        }

        [Fact]
        public async Task RemovePolicy_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var policyId = 1;
            _mockRepo.Setup(repo => repo.GetAllPolicies()).Returns(new List<Policy>());

            // Act
            var result = await _controller.RemovePolicy(policyId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

    }

}
