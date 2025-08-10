using DotNetReactPortal.Server.Controllers;
using DotNetReactPortal.Server.Database;
using DotNetReactPortal.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DotNetReactPortal.Tests
{
    [TestClass]
    public class TestAuthController
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())   // generating new database to each new unit test to create isolation from other test method
                .Options;
            return new ApplicationDbContext(options);
        }

        [TestMethod]
        public async Task Register_ReturnsOk_ForNewUser()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var controller = new AuthController(context);
            var signUpRequest = new SignUpRequest
            {
                Email = "test1@example.com",
                Password = "secure123"
            };

            // Act
            var result = await controller.Register(signUpRequest);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode ?? 200);
        }

        [TestMethod]
        public async Task Register_ReturnsBadRequest_IfEmailExists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Users.Add(new User
            {
                Email = "duplicate@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("pass123")
            });
            await context.SaveChangesAsync();

            var controller = new AuthController(context);
            var signUpRequest = new SignUpRequest
            {
                Email = "duplicate@example.com",
                Password = "newpass"
            };

            // Act
            var result = await controller.Register(signUpRequest);

            // Assert
            var badRequest = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequest);
            Assert.AreEqual(400, badRequest.StatusCode);
        }

        [TestMethod]
        public async Task Login_ReturnsBadRequest_IfPasswordWrong()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            // removing add user options since it is using same database where the values are already input via register
            // function in above test method
            context.Users.Add(new User
            {
                Email = "duplicate@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("newpassword")
            });
            context.SaveChanges();

            var controller = new AuthController(context);
            var loginRequest = new LoginRequest
            {
                Email = "duplicate@example.com",
                Password = "newpassword1"
            };

            // Act
            var result = controller.Login(loginRequest);

            // Assert
            var unauthorized = result as UnauthorizedObjectResult;
            Assert.IsNotNull(unauthorized);
            Assert.AreEqual(401, unauthorized.StatusCode);

        }

        [TestMethod]
        public async Task Login_ReturnsSuccess_IfPasswordCorrect()
        {
            // Arrange
            var context = GetInMemoryDbContext();

            // removing add user options since it is using same database where the values are already input via register
            // function in above test method
            context.Users.Add(new User
            {
                Email = "duplicate@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("newpassword")
            });
            context.SaveChanges();

            var controller = new AuthController(context);
            var loginRequest = new LoginRequest
            {
                Email = "duplicate@example.com",
                Password = "newpassword"
            };

            // Act
            var result = controller.Login(loginRequest);

            // Assert
            var authorized = result as OkObjectResult;
            Assert.IsNotNull(authorized);
            Assert.AreEqual(200, authorized.StatusCode);

        }

    }
}
