using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MoneyTrack.Api.Presentation.Controllers;

[Authorize]
public abstract class AuthenticatedControllerBase : ControllerBase
{
    protected int CurrentUserId
    {
        get
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(value, out var userId) || userId <= 0)
                throw new UnauthorizedAccessException("Usuário não autenticado.");

            return userId;
        }
    }
}
