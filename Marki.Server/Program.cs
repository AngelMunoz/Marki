using Marki.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Cors;

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
    var allTags = tags.Select(doc => doc["Tags"].AsString);
    return new HashSet<string>(allTags);
});

app.MapGet("/blogposts", async (int? page, int? limit, string? title, IEnumerable<string>? tags) =>
    {
        var _page = page ?? 1;
        var _limit = limit ?? 10;
        var enumeratedTags = (tags ?? Array.Empty<string>()).ToHashSet();
        var isTagsEmpty = enumeratedTags.ToArray().Length <= 0;


        var filter = Builders<BlogPost>.Filter.Empty;
        if (!string.IsNullOrWhiteSpace(title) && !isTagsEmpty)
        {
            
            var textFilter = Builders<BlogPost>.Filter.Text(title, new TextSearchOptions { CaseSensitive = false, DiacriticSensitive = false });
            var tagsFilter = Builders<BlogPost>.Filter.Where(post => post.Tags.ToHashSet().Overlaps(enumeratedTags));
            filter = Builders<BlogPost>.Filter.And(textFilter, tagsFilter);
        }
        else if (!isTagsEmpty && string.IsNullOrWhiteSpace(title))
        {
            filter = Builders<BlogPost>.Filter.Where(post => post.Tags.ToHashSet().Overlaps(enumeratedTags));
        }
        else if (isTagsEmpty && !string.IsNullOrWhiteSpace(title))
        {
            filter = Builders<BlogPost>.Filter.Text(title, new TextSearchOptions { CaseSensitive = false, DiacriticSensitive = false });
        }


        var query =
            posts
                ?.Find(filter)
                ?.Skip(_limit * (_page - 1))
                ?.Limit(_limit);
        var items = Enumerable.Empty<BlogPost>();
        var count = 0L;

        if (query is null) return Results.Ok(new PaginatedResponse<BlogPost>(items.ToArray(), count));

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
