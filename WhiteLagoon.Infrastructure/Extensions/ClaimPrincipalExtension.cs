using System.Security.Claims;

namespace WhiteLagoon.Infrastructure.Extentions
{
    public static class ClaimPrincipalExtension
    {
        public static string GetUserName(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Name)?.Value;
        }
        public static string GetUserId(this ClaimsPrincipal user)
        {
            return  user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

    }
}
