using Authorization.AuthModels;
using Authorization.Context;
using Authorization.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.Services
{
    public class JWTService : IJWTService
    {
        private readonly ApplicationDbContext _context;
        private IConfiguration _configuration;
        private IConfiguration @object;

        public JWTService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public JWTService()
        {
        }

        public JWTService(IConfiguration @object)
        {
            this.@object = @object;
        }

        public async Task<AuthResponse> GetRefreshTokenAsync(string ipAddress, int portfolioId, string userName)
        {
            var refreshToken = GenerateRefreshToken();
            var accessToken = GenerateToken(userName);
            return await SaveTokenDetails(ipAddress, portfolioId, accessToken, refreshToken);

        }

        public async Task<AuthResponse> GetTokenAsync(AuthRequest authRequest, string ipAddress)
        {
            var user = _context.Users.FirstOrDefault(x => x.UserName.Equals(authRequest.UserName)
            && x.Password.Equals(authRequest.Password));
            if (user == null)
                return await Task.FromResult<AuthResponse>(null);
            string tokenString = GenerateToken(user.UserName);
            string refreshToken = GenerateRefreshToken();
            return await SaveTokenDetails(ipAddress, user.PortfolioId, tokenString, refreshToken);

        }
        private async Task<AuthResponse> SaveTokenDetails(string ipAddress, int portfolioId, string tokenString, string refreshToken)
        {
            var userRefreshToken = new UserRefreshToken
            {
                CreateDate = DateTime.UtcNow,
                ExpirationDate = DateTime.UtcNow.AddMinutes(5),
                IpAddress = ipAddress,
                IsInvalidated = false,
                RefreshToken = refreshToken,
                Token = tokenString,
                PortfolioId = portfolioId
            };
            await _context.UserRefreshTokens.AddAsync(userRefreshToken);
            await _context.SaveChangesAsync();

            return new AuthResponse { Token = tokenString, RefreshToken = refreshToken, IsSuccess = true };
        }

        private string GenerateRefreshToken()
        {
            var byteArray = new byte[64];
            using (var cryptoProvider = new RNGCryptoServiceProvider())
            {
                cryptoProvider.GetBytes(byteArray);
                return Convert.ToBase64String(byteArray);
            }
        }

        private string GenerateToken(string username)
        {
            var jwtkey = _configuration.GetValue<string>("JwtSettings:Key");
            var keyBytes = Encoding.ASCII.GetBytes(jwtkey);

            var tokenHandler = new JwtSecurityTokenHandler();
            var audinc = _configuration.GetValue<string>("JwtSettings:Audience");
            var descriptor = new SecurityTokenDescriptor()
            {
                Audience = audinc,//inserted
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, username)
                }),
                Expires = DateTime.UtcNow.AddSeconds(900),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(descriptor);
            string tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }
    }
}
