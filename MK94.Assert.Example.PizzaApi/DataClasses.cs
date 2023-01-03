namespace MK94.Assert.Example.PizzaApi
{
    public class Basket
    {
        public List<Pizza> Pizzas { get; set; }
    }

    public class Pizza
    {
        public List<string> Toppings { get; set; }
    }
}
