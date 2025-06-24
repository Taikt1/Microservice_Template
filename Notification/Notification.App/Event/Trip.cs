using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;

namespace Notification.App.Event
{
    public class Trip
    {
        //[JsonIgnore]
        public string? Id { get; set; }
        public string? CustomerId { get; set; }           // Mã khách hàng
        public string? DriverId { get; set; }            // Mã tài xế (nullable trước khi ghép)
        public string? PickupLocation { get; set; }     // Điểm đón (có thể là JSON string hoặc plain text)
        public string? DropoffLocation { get; set; }    // Điểm đến

        public bool? Status { get; set; }         // Trạng thái cuốc xe

        public decimal Price { get; set; }             // Giá ước tính
        //public decimal? FinalFare { get; set; }        // Giá cuối cùng (nullable đến khi kết thúc)

        public DateTime? StartTime { get; set; }       // Thời điểm bắt đầu
        public DateTime? EndTime { get; set; }         // Thời điểm kết thúc
        public DateTime CreatedAt { get; set; }        // Thời điểm tạo
    }

}
