using JamunaERP.Application.Products.Queries.GetProducts;
using JamunaERP.Application.UnitTests.Helpers;

namespace JamunaERP.Application.UnitTests.Products.Queries;

public class GetProductsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsAllProducts_WhenProductsExist()
    {
        using var db = TestDb.Create();
        await TestDb.AddProductAsync(db, price: 25000m, name: "LED TV 32 inch");
        await TestDb.AddProductAsync(db, price: 65000m, name: "Smart Refrigerator");
        var handler = new GetProductsQueryHandler(db);

        var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count());
    }
}