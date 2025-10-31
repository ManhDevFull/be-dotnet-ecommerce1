using Microsoft.EntityFrameworkCore;
using be_dotnet_ecommerce1.Data;
using dotnet.Dtos;
using dotnet.Repository.IRepository;
using Npgsql;
// (Đảm bảo đã thêm 2 using này)
using System.Collections.Generic;
using System.Threading.Tasks;

namespace dotnet.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ConnectData _connect;
        public OrderRepository(ConnectData connect)
        {
            _connect = connect;
        }

        public async Task<IEnumerable<OrderHistoryDTO>> GetOrderHistoryAsync(int accountId)
        {
            // Câu SQL của bạn (giữ nguyên)
            var sql = @"
                SELECT
                    o.id AS ""OrderId"",
                    o.orderdate AS ""OrderDate"",
                    o.statusorder AS ""StatusOrder"",
                    o.quantity * (
                        CASE
                            WHEN d.typediscount = 1 THEN ROUND((v.price * (1 - COALESCE(d.discount, 0)::NUMERIC / 100.0))::NUMERIC)
                            WHEN d.typediscount = 2 THEN v.price - COALESCE(d.discount, 0)
                            ELSE v.price
                        END
                    ) AS ""TotalPriceAfterDiscount""
                FROM orders o
                JOIN variant v ON o.variant_id = v.id
                JOIN product p ON v.product_id = p.id
                LEFT JOIN discount_product dp ON v.id = dp.variant_id
                LEFT JOIN discount d ON dp.discount_id = d.id
                    AND o.orderdate BETWEEN d.starttime AND d.endtime
                WHERE o.account_id = @accountId
                ORDER BY o.id DESC;
            ";

            var accountIdParam = new NpgsqlParameter("@accountId", accountId);

            // === SỬA LẠI DÒNG NÀY ===
            // Thay _connect.Set<OrderHistoryDTO>().FromSqlRaw(...)
            var orders = await _connect.Database
                                   .SqlQueryRaw<OrderHistoryDTO>(sql, accountIdParam) // Dùng SqlQueryRaw
                                   .AsNoTracking()
                                   .ToListAsync();
            // ======================
            foreach (var o in orders)
            {
                Console.WriteLine($"ID={o.OrderId}, Price={o.TotalPriceAfterDiscount}");
            }


            return orders;
        }
    }
}