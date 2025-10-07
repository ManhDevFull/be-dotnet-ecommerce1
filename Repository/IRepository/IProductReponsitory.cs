using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using dotnet.Dtos;

namespace dotnet.Repository.IRepository
{
  public interface IProductReponsitory
  {
    public List<ProductDTO> getProductAdmin(int page, int size);
  }
}