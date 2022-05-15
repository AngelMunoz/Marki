using Marki.Core;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(o => o.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
}

var mongoSettings =
    new MongoClientSettings
    {
        ConnectTimeout = TimeSpan.FromSeconds(5),
        Server = new MongoServerAddress("192.168.100.6")
    };

var database = new MongoClient(mongoSettings).GetDatabase("marki");
var posts = database.GetCollection<BlogPost>("blogposts");

app.MapGet("/tags", async () =>
{
    PipelineDefinition<BlogPost, BsonDocument> pipeline = new BsonDocument[]
    {
        new() { { "$unwind", "$Tags" } }
    };
    var tags = await posts.Aggregate(pipeline).ToListAsync();
    return tags.Select(doc => doc["Tags"].AsString).ToHashSet();
    ;
});

app.MapGet("/blogposts", async (int? page, int? limit, string? title, HttpContext ctx) =>
    {
        var (reqPage, reqLimit, tags) = (page ?? 1, limit ?? 10, new HashSet<string>());
        if (ctx.Request.Query.TryGetValue("tags", out var values))
        {
            foreach (var tag in values)
            {
                tags.Add(tag);
            }
        }

        var (isTagsEmpty, filter) = (tags.Count <= 0, Builders<BlogPost>.Filter.Empty);

        var textFilter = Builders<BlogPost>.Filter.Text(title,
            new TextSearchOptions { CaseSensitive = false, DiacriticSensitive = false });
        var tagsFilter = Builders<BlogPost>.Filter.All("Tags", tags);

        filter = (string.IsNullOrWhiteSpace(title), isTagsEmpty) switch
        {
            (true, false) => tagsFilter,
            (false, true) => textFilter,
            (false, false) => Builders<BlogPost>.Filter.And(textFilter, tagsFilter),
            (true, true) => filter
        };

        var query =
            posts
                ?.Find(filter)
                ?.Skip(reqLimit * (reqPage - 1))
                ?.Limit(reqLimit);
        var (items, count) = (Enumerable.Empty<BlogPost>(), 0L);

        if (query is null) return Results.Ok(new PaginatedResponse<BlogPost>(items, count));

        items = await query.ToListAsync();
        count = posts?.CountDocuments(filter) ?? 0;

        return Results.Ok(new PaginatedResponse<BlogPost>(items, count));
    })
    .WithName("Get Blog Posts");

app.MapPost("/blogposts", async (BlogPostPayload payload) =>
    {
        var post = new BlogPost(
            Guid.NewGuid(),
            payload.Title,
            payload.Content,
            payload.Tags,
            DateTime.Now,
            null);
        await posts.InsertOneAsync(post);
        return Results.Created($"/logposts/{post._id}", post);
    })
    .WithName("Create blog posts");

app.MapPut("/blogposts", async (BlogPost post) =>
    {
        await posts.FindOneAndReplaceAsync(p => p._id == post._id, post.WithUpdate());
        return Results.NoContent();
    })
    .WithName("Update blog posts");

app.Run();
