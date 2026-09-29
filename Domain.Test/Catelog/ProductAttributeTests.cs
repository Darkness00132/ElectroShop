using Domain.Entities.Catalog;
using Domain.Exceptions;
using FluentAssertions;

namespace Domain.Test.Catelog;

public class ProductAttributeTests
{
    private readonly Product _product = CreateProduct();

    [Fact]
    public void An_Attribute_Can_Be_Added_To_A_Product()
    {
        _product.AddAttribute("RAM", "16GB");

        _product.Attributes.Should().ContainSingle();
        _product.Attributes.Single().Name.Should().Be("RAM");
        _product.Attributes.Single().Value.Should().Be("16GB");
    }

    [Fact]
    public void Adding_An_Attribute_With_An_Existing_Name_Updates_Its_Value()
    {
        _product.AddAttribute("RAM", "16GB");

        _product.AddAttribute("RAM", "32GB");

        _product.Attributes.Should().ContainSingle();
        _product.Attributes.Single().Value.Should().Be("32GB");
    }

    [Fact]
    public void An_Attribute_Cannot_Be_Added_With_A_Blank_Name()
    {
        var act = () => _product.AddAttribute(" ", "16GB");

        act.Should().Throw<DomainException>();
        _product.Attributes.Should().BeEmpty();
    }

    [Fact]
    public void An_Attribute_Cannot_Be_Added_With_A_Blank_Value()
    {
        var act = () => _product.AddAttribute("RAM", " ");

        act.Should().Throw<DomainException>();
        _product.Attributes.Should().BeEmpty();
    }

    [Fact]
    public void An_Attribute_Can_Be_Removed_From_A_Product()
    {
        _product.AddAttribute("RAM", "16GB");

        _product.RemoveAttribute("RAM");

        _product.Attributes.Should().BeEmpty();
    }

    [Fact]
    public void Removing_A_Missing_Attribute_Leaves_The_Product_Unchanged()
    {
        _product.AddAttribute("RAM", "16GB");

        var act = () => _product.RemoveAttribute("Color");

        act.Should().NotThrow();
        _product.Attributes.Should().ContainSingle();
    }

    private static Product CreateProduct()
    {
        return new Product(
            "Gaming Laptop",
            "لابتوب ألعاب",
            "A laptop for gaming.",
            "لابتوب للألعاب.",
            "SKU-001",
            1000m,
            Guid.NewGuid(),
            Guid.NewGuid());
    }
}
