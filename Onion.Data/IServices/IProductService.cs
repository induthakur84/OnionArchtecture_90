using OnionArchitecture.DTO;

namespace OnionArchitecture.Data.IServices
{
    public interface IProductService
    {

        Task<ProductResponseDto> Create(ProductRequestDto productRequestDto);
        Task<ProductResponseDto> GetbyID(int id);

        Task<List<ProductResponseDto>> Getall();


        Task<ProductResponseDto> Update(ProductUpdateDto productUpdateDto);

        Task<bool> Delete(int id);
    }
}
