using ReusableDotNet.Pagination;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

var sampleItems = Enumerable.Range(1, 100).Select(static x => $"Item-{x}").ToArray();
var paginator = new Paginator<string>(sampleItems, defaultPageSize: 10, outOfRangeBehavior: PageOutOfRangeBehavior.ClampToLast);
PageResult<string>? currentPage = null;

app.MapGet("/api/pagination/config", () =>
{
    return Results.Ok(new
    {
        paginator.DefaultPageSize,
        paginator.OutOfRangeBehavior
    });
})
.WithName("GetPaginationConfiguration");
//.WithOpenApi();

app.MapPost("/api/pagination/default-page-size/{pageSize:int}", (int pageSize) =>
{
    paginator.SetDefaultPageSize(pageSize);
    return Results.Ok(new
    {
        Message = "Default page size updated.",
        paginator.DefaultPageSize
    });
})
.WithName("SetDefaultPageSize");
//.WithOpenApi();

app.MapGet("/api/pagination/pages/{pageNumber:int}", (int pageNumber, int? pageSize) =>
{
    var page = pageSize.HasValue
        ? paginator.GetPage(pageNumber, pageSize.Value)
        : paginator.GetPage(pageNumber);

    currentPage = page;
    return Results.Ok(page);
})
.WithName("GetPage");
//.WithOpenApi();

app.MapPost("/api/pagination/next", () =>
{
    if (currentPage is null)
    {
        return Results.BadRequest("Call GET /api/pagination/pages/{pageNumber} first.");
    }

    currentPage = paginator.Next(currentPage);
    return Results.Ok(currentPage);
})
.WithName("GetNextPage");
//.WithOpenApi();

app.MapPost("/api/pagination/previous", () =>
{
    if (currentPage is null)
    {
        return Results.BadRequest("Call GET /api/pagination/pages/{pageNumber} first.");
    }

    currentPage = paginator.Previous(currentPage);
    return Results.Ok(currentPage);
})
.WithName("GetPreviousPage");
//.WithOpenApi();

app.MapPost("/api/pagination/next/external", (ExternalPageRequest request) =>
{
    var externalPage = new PageResult<string>(
        request.Items,
        request.PageNumber,
        request.PageSize,
        request.TotalCount,
        request.TotalPages);

    var nextPage = paginator.Next(externalPage);
    return Results.Ok(nextPage);
})
.WithName("TryNextWithExternalPage");
//.WithOpenApi();

app.Run();

public sealed record ExternalPageRequest(
    IReadOnlyList<string> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
