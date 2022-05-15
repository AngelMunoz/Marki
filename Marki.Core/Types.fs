namespace Marki.Core

open System
open MongoDB.Bson.Serialization.Attributes
open MongoDB.Bson.Serialization.IdGenerators

type BlogPost =
    { [<BsonId(IdGenerator = typedefof<CombGuidGenerator>)>]
      _id: Guid
      Title: string
      Content: string
      Tags: string seq
      CreatedAt: DateTime
      UpdateAt: Nullable<DateTime> }

    member this.WithUpdate() =
        { this with UpdateAt = DateTime.Now |> Nullable }

type BlogPostPayload =
    { Title: string
      Content: string
      Tags: string seq }

[<Struct>]
type PaginatedResponse<'Item> = { Items: 'Item seq; Count: int64 }
