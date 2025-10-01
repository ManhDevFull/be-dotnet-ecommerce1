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
                SELECT key, array_agg(DISTINCT value) AS values
                FROM category JOIN product ON (category.id = product.id)
                JOIN variant on (product.id = category.id)
                AND category.id = {id} ,
                    jsonb_each_text(valuevariant)
                GROUP BY key;")
            .ToList();
            return data;
        }

    }
}