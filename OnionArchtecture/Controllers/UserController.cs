using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnionArchitecture.Data.IServices;
using OnionArchitecture.DTO;

namespace OnionArchitecture.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        // dependency injection
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserRegisterDTO userRegisterDTO)
        {
            var result = await _userService.Register(userRegisterDTO);
            return Ok(result);
        }

        [HttpPost("Login")]

        public async Task<IActionResult> Login (LoginRegisterDto loginRegisterDto)
        {
            try
            {
                var result = await _userService.Login(loginRegisterDto);
                return Ok(new
                {
                    message = "LoginSuccessfully",
                    Token=result.Token,
                    User=result.User

                }
                );
            }
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    message = "Login Failed",
                    error = ex.Message

                });

            }
        }
    }
}
