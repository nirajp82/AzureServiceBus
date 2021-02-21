using System;

namespace MessageEntity
{
    public class PizzaOrder
    {
        public string CustomerName { get; set; }
        public string Type { get; set; }
        public string Size { get; set; }
        public override string ToString()
        {
            return $"CustomerName: {CustomerName} Type:{Type} Size:{Size}";
        }
    }
}
