using Authorization.AuthModels;
using Authorization.Context;
using Authorization.Models;
using Authorization.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        static readonly log4net.ILog _log4net = log4net.LogManager.GetLogger(typeof(AccountController));
        private readonly IJWTService _jwtService;
        private readonly ApplicationDbContext _context;
        public AccountController(IJWTService jwtService, ApplicationDbContext context)
        {
            _jwtService = jwtService;
            _context = context;
        }
              

        [HttpPost("[action]")]
        public async Task<IActionResult> AuthToken([FromBody] AuthRequest authRequest)
        {
            try
            {
                _log4net.Info("AuthRequest Initiated");
                
                if (!ModelState.IsValid)
                {
                    _log4net.Info("UserName and Password must be provided.");
                    return BadRequest(new AuthResponse { IsSuccess = false, Reason = "UserName and Password must be provided." });
                }

                _log4net.Info("AuthResponse Initiated");
                var authResponse = await _jwtService.GetTokenAsync(authRequest, HttpContext.Connection.RemoteIpAddress.ToString());

                if (authResponse == null)
                {
                    _log4net.Info("Unauthorized User");
                    return Unauthorized();
                }
                else
                {
                    _log4net.Info("Authorized User");
                    return Ok(authResponse);
                }
            }
            catch (Exception exception)
            {
                _log4net.Error("Exception found while authenticating the user=" + exception.Message);
                return new StatusCodeResult(500);

            }
        }

        /*
        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                _log4net.Info("RefreshTokenRequest Initiated");

                if (!ModelState.IsValid)
                {
                    _log4net.Info("Token must be provided");
                    return BadRequest(new AuthResponse { IsSuccess = false, Reason = "Token must be provided" });
                }

                string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
                var token = GetJetToken(request.ExpiredToken);
                var userRefreshToken = _context.UserRefreshTokens.FirstOrDefault(
                    x => x.IsInvalidated == false && x.Token == request.ExpiredToken
                    && x.RefreshToken == request.RefreshToken
                    && x.IpAddress == ipAddress);

                _log4net.Info("AuthResponse for RefreshToken Initiated");
                
                AuthResponse response = ValidateDetails(token, userRefreshToken);
                if (!response.IsSuccess)
                {
                    _log4net.Info("response not successfull");
                    return BadRequest(response);
                }

                userRefreshToken.IsInvalidated = true;
                _context.UserRefreshTokens.Update(userRefreshToken);
                await _context.SaveChangesAsync();

                var userName = token.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.NameId).Value;
                var authResponse = await _jwtService.GetRefreshTokenAsync(ipAddress, userRefreshToken.PortfolioId,
                    userName);

                _log4net.Info("User RefreshToken authResponse Success");
                return Ok(authResponse);
            }
            catch (Exception exception)
            {
                _log4net.Error("Exception found while authenticating the user=" + exception.Message);
                return new StatusCodeResult(500);

            }

        }*/

        private AuthResponse ValidateDetails(JwtSecurityToken token, Models.UserRefreshToken userRefreshToken)
        {

            if (userRefreshToken == null)
                return new AuthResponse { IsSuccess = false, Reason = "Invalid Token Details." };

            if (token.ValidTo > DateTime.UtcNow)
                return new AuthResponse { IsSuccess = false, Reason = "Token Not Expired." };

            if (!userRefreshToken.IsActive)
                return new AuthResponse { IsSuccess = false, Reason = "Refresh Token Expired." };

            return new AuthResponse { IsSuccess = true };
        }

        private JwtSecurityToken GetJetToken(string expiredToken)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ReadJwtToken(expiredToken);
        }

        
    }
}

