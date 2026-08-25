using Form.Entities;

namespace Form.Interface
{
    public interface ITokenService
    {
        (string token, DateTime expiresAt) CreateToken(User user);

    }
}
