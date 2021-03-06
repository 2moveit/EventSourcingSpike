﻿module EventStoreAdapter

open System
open Streamstone
open Microsoft.Azure // Namespace for CloudConfigurationManager
open Microsoft.WindowsAzure.Storage // Namespace for CloudStorageAccount
open Microsoft.WindowsAzure.Storage.Table
open SerializationAdapter

type Data = {Id:Guid; Type:string; Data:string}

type EventEntity(``type``:string, data:string, version:int) =
    new () = EventEntity("", "", 0)
    member val Type = ``type`` with get,set
    member val Data = data with get,set
    member val Version = version with get,set

let private partitionKey (LicenseeId id) = "LicenseeId-" + id.ToString("D")

let prepare ()  = 
    let table = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient().GetTableReference("eventstore")
    // TODO: Das ist ein extra network call. Dieser sollte nur bei DevStorage einmalig bei bei Anwendungsstart ausgeführt werden 
    // andernfalls kann davon ausgegangen werden, dass der table vom Admin angelegt wurde zb #IF DEBUG
    table.CreateIfNotExists() |> ignore 
    table            

let private toEventData ev=
    let id = Guid.NewGuid()
    let properties = {Data.Id = id;Type = ev.GetType().Name; Data = serialize ev}
    EventData(EventId.From(id), EventProperties.From(properties))



let save (id:LicenseeId) (EventVersion expected) (events:Event list) = async{
    let data = events |> List.map toEventData |> List.toArray
    let table = prepare()
    let partition =  Partition(table , partitionKey id)
    
  //  printfn "Writing to new stream in partition %A" stream.Partition //TODO: Wie richtig loggen (fire&forget)
    try
       let! result = Stream.WriteAsync(partition, expected, data)   |> Async.AwaitTask
       printf "Succesfully written to new stream.\r\nEtag: %s, Version: %i" result.Stream.ETag result.Stream.Version //TODO: Wie mit fehler umgehen? 
       return Ok(EventVersion result.Stream.Version)
    with
    | :? ConcurrencyConflictException as ex -> return Error ("Key.ConcurrencyConflict", ex.Message)
    
    }

let private toEvent data :Event=
    let e:Event = deserialize data
    e

let load (id:LicenseeId) = async{
    let table = prepare()
    let partition =  Partition(table , partitionKey id)
    let! s = Stream.ReadAsync<EventEntity> (partition, 1, 100000) |> Async.AwaitTask // TODO: Parameter prüfen/verbessern
    let ev = s.Events |> Array.map (fun ev -> toEvent ev.Data) |> Array.toList
    return ev, EventVersion s.Stream.Version
    }
