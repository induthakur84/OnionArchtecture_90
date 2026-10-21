using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Onion.Data;
using OnionArchitecture.Data.IServices;
using OnionArchitecture.Domain;
using OnionArchitecture.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OnionArchitecture.Data.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        private readonly IConfiguration _configuration;

        public UserService(ApplicationDbContext context, IConfiguration configuration)

        {
            _context = context;
            _configuration = configuration;
        }



        public async Task<UserResponseDto> Register(UserRegisterDTO userRegisterDTO)
        {
            // here we  are using mannual mapping

            // we can map the userrequest dto to the user entity

            var user = new Domain.User
            {
                FirstName = userRegisterDTO.FirstName,
                LastName = userRegisterDTO.LastName,
                UserName = userRegisterDTO.UserName,
                Password = userRegisterDTO.Password,
                ConfirmPassword = userRegisterDTO.ConfirmPassword,
                Role = userRegisterDTO.Role
            };

            //AddUser to the database

            await _context.Users.AddAsync(user);

            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                FirstName = userRegisterDTO.FirstName,
                LastName = userRegisterDTO.LastName,
                UserName = userRegisterDTO.UserName,
                Role = userRegisterDTO.Role
            };

        }


        public async Task<LoginResponseDto> Login(LoginRegisterDto loginRegisterDto)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserName == loginRegisterDto.UserName);

            if (user == null)
            {
                throw new Exception("User not found in the database");
            }
            var token = GenerateToken(user);

            return new LoginResponseDto
            {
                Token = token,
                User = new UserResponseDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Role = user.Role
                }
            };

        }




        private string GenerateToken(User user)
        {

            var jwtSettings = _configuration.GetSection("Jwt");


            //CONVERT KEY TO BYTES AND GERNERAT SYMMERTIC SECUIRTYKEY

            var secretkey =
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

            var signingCredential = new SigningCredentials(secretkey, SecurityAlgorithms.HmacSha256);



            //Payload (user information or user claims)


            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName)
            };


            var durationinMinutes = Convert.ToDouble(jwtSettings["ExpiryMinutes"]);


            var token = new JwtSecurityToken(

                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audiences"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(durationinMinutes),
                signingCredentials: signingCredential
              );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }
    }
}