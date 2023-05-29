using System.Collections.Generic;
using AnySoftBackend.Library.DataTransferObjects.Product;

namespace AnySoftMobile.Models;

public class SearchModel
{
    public string SearchString { get; set; } = "";
    public IEnumerable<ProductResponseDto> Products { get; set; } = new List<ProductResponseDto>();
}