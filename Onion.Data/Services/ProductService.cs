using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Onion.Data;
using OnionArchitecture.Data.IServices;
using OnionArchitecture.Domain;
using OnionArchitecture.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnionArchitecture.Data.Services
{
    public class ProductService : IProductService
    {

        private readonly ApplicationDbContext _context;
        
        public ProductService(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public async Task<ProductResponseDto> Create(ProductRequestDto productRequestDto)
        {
            var product = new Product
            {
                Name = productRequestDto.Name,
                Description = productRequestDto.Description
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description
            };
        }

        public async Task<bool> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if(product==null)
            {
                return false;
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ProductResponseDto>> Getall()
        {
            return await _context.Products.Select(p => new ProductResponseDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description
            }).AsNoTracking().ToListAsync();
        }

        public async Task<ProductResponseDto> GetbyID(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return null;
            }

            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description
            };
        }

        public async Task<ProductResponseDto> Update(ProductUpdateDto productUpdateDto)
        {
            var product = await _context.Products.FindAsync(productUpdateDto.Id);

            if (product == null)
            {
                return null;
            }

            product.Name = productUpdateDto.Name;
            product.Description = product.Description;

            await _context.SaveChangesAsync();
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description
            };
        }
    }
}
