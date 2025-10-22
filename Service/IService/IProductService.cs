using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Dtos;
using dotnet.Model;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace be_dotnet_ecommerce1.Service
{
    public interface IProductService
    {
        public int getQuantityByIdCategory(int id);
        public Task<PagedResultDTO<ProductFilterDTO>> getProductByFilter(FilterDTO dTO);
    }
}