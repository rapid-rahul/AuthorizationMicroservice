using Authorization.Context;
using Authorization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]

    [ApiController]
    public class NamesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(NamesController));

        public NamesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("[action]")]
        public async Task<IEnumerable<User>> GetNames()
        {

            return _context.Users.ToList();
        }
    }
}
