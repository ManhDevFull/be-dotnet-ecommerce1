using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dotnet.Service.IService;

namespace dotnet.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize] // Yêu cầu phải đăng nhập
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet("my-orders")]
        public async Task<IActionResult> GetMyOrderHistory()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out var userId))
            {
                return Unauthorized("Không thể xác định người dùng.");
            }

            try
            {
                var orders = await _orderService.GetOrderHistoryAsync(userId);
                return Ok(orders); // Trả về danh sách đơn hàng
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy lịch sử đơn hàng cho User ID {UserId}", userId);
                return StatusCode(500, new { message = "Lỗi server khi lấy lịch sử đơn hàng." });
            }
        }

        // ✅ THÊM PHƯƠNG THỨC MỚI NÀY VÀO
        [HttpGet("my-orders/{orderId}")]
        [Authorize] // Yêu cầu đăng nhập (bất kỳ role nào)
        public async Task<IActionResult> GetMyOrderDetail(int orderId)
        {
            // 1. Lấy userId từ token
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                _logger.LogWarning("GetMyOrderDetail: Không thể xác định user ID từ token.");
                return Unauthorized("Token không hợp lệ hoặc thiếu thông tin.");
            }

            try
            {
                // 2. Gọi service để lấy chi tiết (dùng chung hàm với Admin)
                var orderDetail = await _orderService.GetOrderDetailAsync(orderId);

                if (orderDetail == null)
                {
                    _logger.LogWarning("GetMyOrderDetail: Không tìm thấy đơn hàng ID {OrderId}", orderId);
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                // 3. ✅ KIỂM TRA BẢO MẬT QUAN TRỌNG
                // Đảm bảo người dùng này CHỈ ĐƯỢC XEM đơn hàng của chính họ
                if (orderDetail.AccountId != userId)
                {
                    _logger.LogWarning("GetMyOrderDetail: User ID {UserId} cố gắng xem đơn hàng {OrderId} của User ID {OwnerId}.", userId, orderId, orderDetail.AccountId);
                    // Trả về 404 (Not Found) thay vì 401 (Unauthorized)
                    // để tránh tiết lộ rằng đơn hàng này có tồn tại.
                    return NotFound(new { message = "Không tìm thấy đơn hàng." });
                }

                // 4. Trả về kết quả
                return Ok(orderDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết đơn hàng {OrderId} cho User ID {UserId}", orderId, userId);
                return StatusCode(500, new { message = "Lỗi server khi lấy chi tiết đơn hàng." });
            }
        }
    }
}