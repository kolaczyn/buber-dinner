using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BuberDinner.Application.Common.Interfaces.Authentication;
using BuberDinner.Application.Common.Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BuberDinner.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IDateTimeProvider dateTimeProvider, IOptions<JwtSettings> jwtOptions)
    {
        _dateTimeProvider = dateTimeProvider;
        _jwtSettings = jwtOptions.Value;
    }

    public string GenerateToken(Guid userId,
        string firstName,
        string lastName)
    {
        var signingCredentials =
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret)),
                SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,
                userId.ToString()),
            new Claim(JwtRegisteredClaimNames.GivenName,
                firstName),
            new Claim(JwtRegisteredClaimNames.FamilyName,
                lastName),
            new Claim(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid()
                    .ToString())
        };

        var securityToken = new JwtSecurityToken(_jwtSettings.Issuer,
            _jwtSettings.Audience,
            claims,
            signingCredentials: signingCredentials,
            expires: _dateTimeProvider.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes));

        return new JwtSecurityTokenHandler().WriteToken(securityToken);
    }
}