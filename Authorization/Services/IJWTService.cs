using Authorization.AuthModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Authorization.Services
{
   public interface IJWTService
    {
        Task<AuthResponse> GetTokenAsync(AuthRequest authRequest, string ipAdress);

        Task<AuthResponse> GetRefreshTokenAsync(string ipAdress, int portfolioId, string userName);
    }
}
