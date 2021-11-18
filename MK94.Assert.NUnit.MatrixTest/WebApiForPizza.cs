using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MK94.Assert.NUnit.MatrixTest
{
    #region data classes

    public class User
    {
        public string Name { get; set; }

        // IMPORTANT: Do not store cleartext password in real systems; Use a password hashing algorithm like pbkdf2 or bcrypt
        public string Password { get; set; }
    }

    public class Cart
    {
        public string User { get; set; }

        public List<Pizza> Pizzas { get; set; }
    }

    public enum Pizza
    {
        Margherita,
        Pizzageddon,
        TheMozzarellaFellas
    }

    public class Order
    {
        public string User { get; set; }

        public DateTime OrderTime { get; set; }

        public List<Pizza> Pizzas { get; set; }
    }

    #endregion

    public class WebApiForPizza
    {
        private readonly InMemoryDb Db;

        public WebApiForPizza(InMemoryDb db)
        {
            Db = db;
        }

        public void RegisterUser(string user, string pass)
        {
            Db.Insert(new User { Name = user, Password = pass });
        }

        public string Login(string user, string pass)
        {
            if (!Db.Select<User>(x => x.Name == user && x.Password == pass).Any())
                throw new ArgumentException("Bad login");

            // In real systems the token should be a cryptographically signed object; See JWT
            var token = user;

            return token;
        }

        public void AddPizza(string token, Pizza type)
        {
            var cart = Db.Select<Cart>(x => x.User == token).SingleOrDefault();

            if (cart == null)
                cart = new Cart { User = token, Pizzas = new List<Pizza>() };

            cart.Pizzas.Add(type);

            Db.Delete<Cart>(x => x.User == token);
            Db.Insert(cart);
        }

        public void RemovePizza(string token, Pizza type)
        {
            var cart = Db.Select<Cart>(x => x.User == token).SingleOrDefault();

            if (cart == null)
                throw new InvalidOperationException($"Cart empty");

            if (!cart.Pizzas.Remove(type))
                throw new InvalidOperationException($"Pizza not in cart");

            Db.Delete<Cart>(x => x.User == token);
            Db.Insert(cart);
        }

        public void FinishOrder(string token)
        {
            var cart = Db.Select<Cart>(x => x.User == token).SingleOrDefault();

            if (cart == null)
                throw new InvalidOperationException($"Cart empty");

            Db.Delete<Cart>(x => x.User == token);
            Db.Insert(new Order
            {
                User = token,
                Pizzas = cart.Pizzas,
                OrderTime = PseudoRandom.DateTime() // Should be replaced by actual DateTime.Now in production use; See IGuidProvider for a similar implementation
            });
        }
    }
}
