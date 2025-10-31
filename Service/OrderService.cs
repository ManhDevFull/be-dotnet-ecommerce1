using dotnet.Dtos;
using dotnet.Repository.IRepository;
using dotnet.Service.IService;

namespace dotnet.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repo;
        public OrderService(IOrderRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<OrderHistoryDTO>> GetOrderHistoryAsync(int accountId)
        {
            return await _repo.GetOrderHistoryAsync(accountId);
        }
    }
}