using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using be_dotnet_ecommerce1.Controllers;
using be_dotnet_ecommerce1.Dtos;
using dotnet.Dtos;

namespace dotnet.Repository.IRepository
{
  public interface IProductReponsitory
  {
    public List<ProductDTO> getProductAdmin(int page, int size);
       public int getQuantityByIdCategory(int id);
        public Task<List<ProductFilterDTO>> getProductByFilter(FilterDTO dTO);
  }
}