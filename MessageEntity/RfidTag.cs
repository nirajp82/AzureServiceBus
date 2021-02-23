using System;
using System.Collections.Generic;
using System.Text;

namespace MessageEntity
{
    public class RfidTag
    {
        public string TagId { get; private set; }
        public string Product { get; set; }
        public double Price { get;  set; }

        public RfidTag()
        {
            TagId = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return $"Product: {Product} \t Price:{Price}";
        }

        public static RfidTag[] CreateBasket() 
        {
            RfidTag[] basket = new RfidTag[]
            {
                new RfidTag {Product = "Apples", Price = 1.99},
                new RfidTag {Product = "Apricots", Price = 2.98},
                new RfidTag {Product = "Avocados", Price = 3.97},
                new RfidTag {Product = "Cantaloupe", Price = 4.96},
                new RfidTag {Product = "Clementine", Price = 5.95},

                new RfidTag {Product = "Huckleberry", Price = 6.81},
                new RfidTag {Product = "Entawak", Price = 7.82},
                new RfidTag {Product = "Eggfruit", Price = 4.83},
                new RfidTag {Product = "Dewberries", Price = 4.84},
                new RfidTag {Product = "Plum", Price = 2.85},

                new RfidTag {Product = " Bing Cherry", Price = 1.73},
            };
            return basket;
        }
    }
}
