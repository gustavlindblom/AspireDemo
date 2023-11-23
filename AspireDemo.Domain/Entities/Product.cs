using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspireDemo.Domain.Models
{
  public class Product(int productid, string productname, string productdescription, string slug)
  {
    public int ProductId { get; set; } = productid;
    public string ProductName { get; set; } = productname;
    public string ProductDescription { get; set; } = productdescription;
    public string Slug { get; set; } = slug;
  }
}
