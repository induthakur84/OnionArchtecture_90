using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnionArchitecture.Data.IServices;
using OnionArchitecture.DTO;

namespace OnionArchitecture.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        public readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }


        [HttpPost("Create")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create (ProductRequestDto productRequestDto)
        {
            var result = await _productService.Create(productRequestDto);
            return Ok(result);
        }

        [HttpGet("GetAll")]
        [Authorize(Roles= "Employee")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _productService.Getall();
            return Ok(result);
        }

        [HttpGet("{id}")]

        [Authorize(Roles = "Student")]

        public async Task<IActionResult> GetById(int id)
        {
            var result = await _productService.GetbyID(id);
            if (result == null)
            {
                return NotFound();

            }

            return Ok(result);
        }

        [HttpPut("Update")]

        [Authorize(Policy ="MainUser")]
        public async Task<IActionResult> Update(ProductUpdateDto productUpdateDto)
        {
            var result = await _productService.Update(productUpdateDto);
            if(result==null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpDelete("Delete")]

        [Authorize(Roles ="Admin")]

        public async Task<IActionResult> Delete(int id)
        {
            var result = await _productService.Delete(id);

            if(result == false)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
