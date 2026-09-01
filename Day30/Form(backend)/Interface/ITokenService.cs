using Form.Entities;

namespace Form.Interfaces
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) CreateToken(User user);

    }
}
