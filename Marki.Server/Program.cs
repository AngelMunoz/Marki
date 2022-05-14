
using Marki.Core;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var database = new MongoClient("mongodb://192.168.100.6/marki")?.GetDatabase("marki");

IMongoCollection<TType>? GetMongoCollection<TType>()
{
    return database?.GetCollection<TType>("blogposts");
}

;


app.MapGet("/blogposts", (int? page, int? limit) =>
    {
        var _page = page ?? 1;
        var _limit = limit ?? 10;
        var posts = GetMongoCollection<BlogPost>();
        var items = posts.AsQueryable().Skip(_limit * (_page - 1)).Take(_limit).ToArray();
    return Results.Ok(new PaginatedResponse<BlogPost> (Array.Empty<BlogPost>(),0L));
})
    .Produces(200, typeof(PaginatedResponse<BlogPost))
    .WithName("Get Blog Posts");

app.Run();
