using OnionArchitecture.DTO;

namespace OnionArchitecture.Data.IServices
{
    public  interface IUserService
    {
        Task<UserResponseDto> Register(UserRegisterDTO userRegisterDTO);
        Task<LoginResponseDto> Login(LoginRegisterDto loginRegisterDto);
    }
}
