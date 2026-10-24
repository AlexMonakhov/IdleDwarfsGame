public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}
