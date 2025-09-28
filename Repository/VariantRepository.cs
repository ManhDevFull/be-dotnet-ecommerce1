using be_dotnet_ecommerce1.Data;
using be_dotnet_ecommerce1.Dtos;
using Microsoft.EntityFrameworkCore;

namespace be_dotnet_ecommerce1.Repository.IRepository
{
    public class VariantRepository : IVariantRepository
    {
        private readonly ConnectData _connect;
        public VariantRepository(ConnectData connect)
        {
            _connect = connect;
        }
        public List<VariantDTO> getValueVariant(int id)
        {
                var data = _connect.Database
                .SqlQuery<VariantDTO>($@"
                SELECT DISTINCT 
                    variant.id AS Id,
                    jsonb_object_keys(variant.data) AS ValueVariant
                FROM category
                JOIN product ON category.id = product.categoryid
                JOIN variant ON product.id = variant.productid
                WHERE category.id = {id}")
                .ToList();
                return data;
        }

    }
}