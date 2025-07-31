using DotNetReactPortal.Server.Controllers;
using DotNetReactPortal.Server.Database;
using DotNetReactPortal.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DotNetReactPortal.Tests
{
    [TestClass]
    public class TestUserController
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
            return new ApplicationDbContext(options);
        }

        private ClaimsPrincipal GetMockUser(string email)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, email) // optional
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            return new ClaimsPrincipal(identity);
        }

        [TestMethod]
        public async Task GetUserProfile_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            // Seed test user into the DB
            var testUser = new User
            {
                Email = "test@example.com",
                Password = "hashedpassword"
            };
            context.Users.Add(testUser);
            await context.SaveChangesAsync();

            var controller = new UserController(context);

            // Simulate authenticated user
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = GetMockUser("test@example.com")
                }
            };

            // Act
            var result = await controller.GetUserProfile();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedUser = okResult.Value as User;
            Assert.IsNotNull(returnedUser);
            Assert.AreEqual("test@example.com", returnedUser.Email);
        }

        [TestMethod]
        public async Task GetUserProfile_ShouldReturnUnauthorized_WhenEmailClaimMissing()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new UserController(context);

            // Simulate user with no email claim
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity()) // empty identity
                }
            };

            // Act
            var result = await controller.GetUserProfile();

            // Assert
            Assert.IsInstanceOfType(result, typeof(UnauthorizedObjectResult));
        }

        [TestMethod]
        public async Task GetUserProfile_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new UserController(context);

            // Simulate authenticated user with an email not in DB
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = GetMockUser("notfound@example.com")
                }
            };

            // Act
            var result = await controller.GetUserProfile();

            // Assert
            Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        }
    }
}
