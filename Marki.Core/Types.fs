namespace Marki.Core

open System
open MongoDB.Bson
open MongoDB.Bson.Serialization.Attributes

type BlogPost =
    { [<BsonId>]
      _id: ObjectId
      Title: string
      Content: string
      Tags: string[]
      CreatedAt: DateTime }

[<Struct>]
type PaginatedResponse<'Item> = { Items: 'Item[]; Count: int64 }
