using System;
using System.Threading.Tasks;
using dotnet.Dtos.admin;
using dotnet.Repository.IRepository;
using dotnet.Service.IService;

namespace dotnet.Service
{
  public class OrderService : IOrderService
  {
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
      _repository = repository;
    }

    public Task<PagedResult<OrderAdminDTO>> GetOrdersAsync(
        int page,
        int size,
        string? status,
        string? payment,
        string? payType,
        string? keyword,
        DateTime? fromDate,
        DateTime? toDate)
    {
      return _repository.GetOrdersAsync(page, size, status, payment, payType, keyword, fromDate, toDate);
    }

    public Task<OrderAdminDTO?> GetOrderDetailAsync(int orderId)
    {
      return _repository.GetOrderDetailAsync(orderId);
    }

    public Task<bool> UpdateOrderStatusAsync(int orderId, string status, string? paymentStatus)
    {
      return _repository.UpdateOrderStatusAsync(orderId, status, paymentStatus);
    }

    public Task<OrderAdminSummaryDTO> GetSummaryAsync()
    {
      return _repository.GetSummaryAsync();
    }
  }
}
