namespace WebShop.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int QuantityInStock { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}

