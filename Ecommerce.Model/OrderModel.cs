﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Model
{
    public class OrderModel
    {
        [Key]
        public int Id { get; set; }
        public string CustomerName { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public DateTime OrderDate { get; set; }
    }
}
