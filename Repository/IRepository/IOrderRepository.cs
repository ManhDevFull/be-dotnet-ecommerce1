using dotnet.Dtos;
namespace dotnet.Repository.IRepository
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderHistoryDTO>> GetOrderHistoryAsync(int accountId);
    }
}