﻿using DotNetReactPortal.Server.Controllers;
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
                .UseInMemoryDatabase(databaseName: "TestDb")
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
                Email = "test@example.com",
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
    }
}
