using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeslaMed.Controllers;
using TeslaMed.Models;
using TeslaMed.Models.Repositories;
using Xunit;

namespace TeslaMed.Tests
{
    public class DiscountControllerTests
    {
        [Fact]
        public void DeleteDiscountReturnsAViewResultWithAllListOfDiscounts()
        {
            int discountId = 3;
            var mock = new Mock<IRepository>();
            mock.Setup(repo => repo.GetAll<Discount>("Discounts")).ReturnsDbSet(GetTestDiscounts());
            var controller = new DiscountsController(null, null, mock.Object);
            var discount = mock.Object.GetAll<Discount>("Discounts").FirstOrDefault(d => d.Id == discountId);

            var result = controller.Delete(discountId).Result;

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Null(redirectToActionResult.ControllerName);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Null(redirectToActionResult.RouteValues);
            mock.Verify(r => r.DbRemove(It.IsAny<Discount>()), Times.Once());
            mock.Verify(r => r.DbSave());
        }

        private DbSet<Discount> GetTestDiscounts()
        {
            var discounts = new List<Discount>()
            {
                new Discount()
                {
                    Id = 1,
                    Name = "Dis_1",
                    Percent = 15
                },
                new Discount()
                {
                    Id = 2,
                    Name = "Dis_2",
                    Percent = 30
                },
                new Discount()
                {
                    Id = 3,
                    Name = "Dis_3",
                    Percent = 45
                },
            };
            return GetQueryableMockDbSet(discounts);
        }

        private static DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();

            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));

            return dbSet.Object;
        }
    }
}
