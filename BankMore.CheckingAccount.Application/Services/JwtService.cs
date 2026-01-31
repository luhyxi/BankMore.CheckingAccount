using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankMore.CheckingAccount.Domain.ContaCorrenteAggregate;
using BankMore.CheckingAccount.Domain.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using ContaCorrenteModel = BankMore.CheckingAccount.Domain.ContaCorrenteAggregate.ContaCorrente;

namespace BankMore.CheckingAccount.Application.Services;

public sealed class JwtService : IJwtService
{
    private readonly JwtOptions _options;
    private readonly JwtSecurityTokenHandler _tokenHandler = new();

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(ContaCorrenteModel conta)
    {
        ArgumentNullException.ThrowIfNull(conta);

        if (string.IsNullOrWhiteSpace(_options.SigningKey))
        {
            throw new InvalidOperationException("JWT signing key is not configured.");
        }

        var claims = new List<Claim>
        {
            new("id", conta.Id.Value.ToString()),
            new(JwtRegisteredClaimNames.Name, conta.Nome.Value),
            new("numero", conta.Numero.Value)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return _tokenHandler.WriteToken(token);
    }
}
