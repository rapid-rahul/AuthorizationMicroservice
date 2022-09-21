using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using Authorization.AuthModels;
using Authorization.Controllers;
using Authorization.Models;
using Authorization.Services;
using Authorization.Context;

namespace AuthorizationTest
{
    public class Tests
    {
        private Mock<IConfiguration> _config;
        private JWTService _service;
        private AccountController _controller;
        private ApplicationDbContext _context;
        
        [SetUp]
        public void Setup()
        {
            _config = new Mock<IConfiguration>();
            _service = new JWTService();
            
        }

        [Test]
        public void AuthTokenTest_Pass()
        {
            _controller = new AccountController(_service, _context);
            var result = _controller.AuthToken;
            Assert.IsNotNull(result);
        }
        [Test]
        public void AuthTokenTest_Fail()
        {
            _controller = new AccountController(_service, _context);
            var result = _controller.AuthToken;
            Assert.IsNull(result);
        }
        [Test]
        public void GetTest_Pass()
        {
            NamesController valuesController = new NamesController(_context);
            var result = valuesController.GetNames;
            Assert.IsNotNull(result);
        }
        [Test]
        public void GetTest_Fail()
        {
            NamesController valuesController = new NamesController(_context);
            var result = valuesController.GetNames;
            Assert.IsNull(result);
        }

    }
}