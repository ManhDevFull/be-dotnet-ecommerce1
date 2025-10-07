using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet.Dtos;

namespace dotnet.Service.IService
{
    public interface IProductService
    {
    public List<ProductDTO> getProductAdmin(int page, int size);
  }
}