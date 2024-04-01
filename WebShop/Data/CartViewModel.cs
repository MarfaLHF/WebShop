namespace WebShop.Data
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
