using Microsoft.AspNetCore.Identity;

namespace WebShop.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public string UserID { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public int TotalAmount { get; set; }

        public IdentityUser User { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
    }
}
