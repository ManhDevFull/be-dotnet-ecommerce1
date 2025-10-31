using dotnet.Dtos;
namespace dotnet.Service.IService
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderHistoryDTO>> GetOrderHistoryAsync(int accountId);
    }
}