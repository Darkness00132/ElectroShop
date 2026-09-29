namespace Application.Features.Products.Dtos;

public class DetailedProduct
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = null!;
    public string NameAr { get; set; } = null!;
    public string DescriptionEn { get; private set; } = null!;
    public string DescriptionAr { get; private set; } = null!;
    public decimal Price { get; set; }
    public string CategoryNameEn { get; set; } = null!;
    public string CategoryNameAr { get; set; } = null!;
    public string BrandNameEn { get; set; } = null!;
    public string BrandNameAr { get; set; } = null!;
    public DiscountInProduct? Discount { get; set; }
    public int Quantity { get; set; }
    public List<ProductImageDto> Images { get; set; } = [];
    public List<ProductAttributeDto> Attributes { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
