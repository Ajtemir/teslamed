using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Moq;
using TeslaMed.Controllers;
using TeslaMed.Models.Repositories;
using TeslaMed.Models;
using Xunit;
using Org.BouncyCastle.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Moq.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Mvc.Routing;
using TeslaMed.Services;
using NSubstitute;
using TeslaMed.ViewModels;
using System.Linq.Expressions;
using FellowOakDicom.Imaging.Reconstruction;

namespace TeslaMed.Tests
{
    public class TestNewsController
    {
        [Fact]
        public async Task IndexReturnsViewResult()
        {
            var mock = new Mock<IRepository>();
            mock.Setup(repo => repo.GetAllNews()).Returns(GetTestNews());
            var controller = new NewsController(null, null, null, mock.Object, null);
            var result = controller.Index(null, null, null);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<News>>(viewResult.Model);
            int count = GetTestNews().Count();
            Assert.Equal(count, model.Count());
        }

        private List<MainDoctor> GetDoctors()
        {
            var docs = new List<MainDoctor>
            {
                new MainDoctor
                {
                    Id = 1,
                    FullName = "Test1",
                    Description = "Test",
                    Image = "~/images/Logo_TESLAMED2.png",
                },
                new MainDoctor
                {
                    Id = 2,
                    FullName = "Test2",
                    Description = "Test",
                    Image = "~/images/Logo_TESLAMED2.png",
                },
            };
            return docs;
        }
        private List<News> GetTestNews()
        {
            var news = new List<News>
            {
                new News
                {
                    Id = 1,
                    Title = "Test1",
                    Description = "Test",
                    Image = "~/images/Logo_TESLAMED2.png",
                    Date = DateTime.Now,
                    EditDate = DateTime.Now,
                    Links = new List<Link>{ new Link { Id = 1, Title = "title1", Url = "url1"}, new Link { Id = 2, Title = "title2", Url = "url2" } }
                },
                new News
                {
                    Id = 2,
                    Title = "Test2",
                    Description = "Test",
                    Image = "~/images/Logo_TESLAMED2.png",
                    Date = DateTime.Now,
                    EditDate = DateTime.Now,
                    Links = new List<Link>{ new Link { Id = 1, Title = "title1", Url = "url1"}, new Link { Id = 2, Title = "title2", Url = "url2" } }
                },
            };
            return news;
        }
        [Fact]
        public void CreateHttpGet_ReturnsViewResult()
        {
            var mockRepo = new Mock<IRepository>();
            var controller = new NewsController(null, null, null, mockRepo.Object, null);

            var result = controller.Create();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<News>(viewResult.Model);
            Assert.NotNull(model);
            Assert.NotNull(model.Links);
        }
        [Fact]
        public void PublicationExists_ReturnsTrue()
        {
            int idToCheck = 1;
            var mockRepo = new Mock<IRepository>();
            mockRepo.Setup(repo => repo.GetAllNews()).Returns(GetTestNews());
            var controller = new NewsController(null, null, null, mockRepo.Object, null);
            var result = controller.PublicationExists(idToCheck);
            Assert.True(result);
        }

        [Fact]
        public void PublicationExists_ReturnsFalse()
        {
            int idToCheck = 99999;
            var mockRepo = new Mock<IRepository>();
            mockRepo.Setup(repo => repo.GetAllNews()).Returns(GetTestNews());
            var controller = new NewsController(null, null, null, mockRepo.Object, null);
            var result = controller.PublicationExists(idToCheck);
            Assert.False(result);
        }
        [Fact]
        public async Task EditHttpGet_ReturnsViewResult()
        {
            var mockRepo = new Mock<IRepository>();
            var controller = new NewsController(null, null, null, mockRepo.Object, null);
            int idToEdit = 1;
            mockRepo.Setup(repo => repo.GetAllNews()).Returns(GetTestNews());
            var result = await controller.Edit(idToEdit);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<News>(viewResult.Model);
            Assert.NotNull(model);
            Assert.NotNull(model.Links);
        }
        [Fact]
        public async Task EditHttpGet_WithNullId_ReturnsNotFound()
        {
            var mockRepo = new Mock<IRepository>();
            var controller = new NewsController(null, null, null, mockRepo.Object, null);
            int? idToEdit = null;
            var result = await controller.Edit(idToEdit);
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Delete_ReturnsRedirectToAction()
        {
            var mockRepo = new Mock<IRepository>();
            var controller = new NewsController(null, null, null, mockRepo.Object, null);
            var newsToDelete = GetTestNews().First();
            mockRepo.Setup(repo => repo.GetAllNews()).Returns(GetTestNews());
            var result = await controller.Delete(newsToDelete.Id);
            mockRepo.Verify(r => r.DbRemove<News>(It.Is<News>(n => n.Id == newsToDelete.Id)), Times.Once);
            mockRepo.Verify(r => r.DbSave(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("News", redirectToActionResult.ControllerName);
        }

        [Fact]
        public void DeleteLink_ReturnsJsonSuccessAndRedirectToAction()
        {
            var mockRepo = new Mock<IRepository>();
            var urlHelperMock = new Mock<IUrlHelper>();
            var controller = new NewsController(null, null, null, mockRepo.Object, null);
            controller.Url = urlHelperMock.Object;
            mockRepo.Setup(repo => repo.GetAllLinks()).Returns(new List<Link> { new Link { Id = 1, Title = "Test Link", Url = "http://example.com" } });
            mockRepo.Setup(r => r.DbRemove(It.IsAny<Link>()));
            mockRepo.Setup(r => r.DbSave());
            urlHelperMock.Setup(u => u.Action(It.IsAny<UrlActionContext>()))
                .Returns("/News/Edit/1");
            var result = controller.DeleteLink(1, 1);
            var jsonResult = Assert.IsType<JsonResult>(result);
            var jsonResultData = jsonResult.Value as IDictionary<string, object>;
            mockRepo.Verify(r => r.DbRemove(It.IsAny<Link>()), Times.Once);
            mockRepo.Verify(r => r.DbSave(), Times.Once);
        }
        [Fact]
        public void AddMainDoctor_Get_ReturnsViewResult()
        {
            var mockRepo = new Mock<IRepository>();
            var controller = new NewsController(null, null, null, mockRepo.Object, null);

            var result = controller.AddMainDoctor();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        [Fact]
        public async Task AddMainDoctor_Post_RedirectsToAction()
        {
            var mockRepo = new Mock<IRepository>();
            var controller = new NewsController(null, null, null, mockRepo.Object, null);

            var result = await controller.AddMainDoctor(new MainDoctor(), null);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MainPage", redirectToActionResult.ActionName);
            Assert.Equal("News", redirectToActionResult.ControllerName);
        }

        [Fact]
        public async Task RemoveMainDoctor_ReturnsRedirectToAction()
        {
            var mockRepo = new Mock<IRepository>();
            var controller = new NewsController(null, null, null, mockRepo.Object, null);
            var newsToDelete = GetDoctors().First();
            mockRepo.Setup(repo => repo.GetMainDoctors()).Returns(GetDoctors());
            var result = await controller.RemoveMainDoctor(newsToDelete.Id);
            mockRepo.Verify(r => r.DbRemove<MainDoctor>(It.Is<MainDoctor>(n => n.Id == newsToDelete.Id)), Times.Once);
            mockRepo.Verify(r => r.DbSave(), Times.Once);
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("MainPage", redirectToActionResult.ActionName);
            Assert.Equal("News", redirectToActionResult.ControllerName);
        }

    }
}
