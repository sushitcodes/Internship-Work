// Product.cs — a plain data class LINQ will query against
    class Product
    {
        public string Name;
        public double Price;
        public string Category;

        public Product(string name, double price, string category)
        {
            Name = name;
            Price = price;
            Category = category;
        }
    }
