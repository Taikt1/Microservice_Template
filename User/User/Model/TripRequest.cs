namespace TestKafka.Model
{
    public class TripRequest
    {
        public string UserId { get; set; }           // ID người gọi xe (bắt buộc)
        public string PickupLocation { get; set; }     // Điểm đón (có thể là JSON string hoặc plain text)
        public string DropoffLocation { get; set; }    // Điểm đến
        public decimal Price { get; set; }             // Giá ước tính
    }
}
