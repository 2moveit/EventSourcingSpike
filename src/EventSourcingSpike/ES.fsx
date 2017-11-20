#if INTERACTIVE
#I @"./../packages"
#r @"Streamstone/lib/netstandard1.6/Streamstone.dll"
#r @"WindowsAzure.Storage\lib\net45\Microsoft.WindowsAzure.Storage.dll"
#r @"Microsoft.WindowsAzure.ConfigurationManager\lib\net40\Microsoft.WindowsAzure.Configuration.dll"
#r @"System.Spatial\lib\portable-net45+wp8+win8+wpa\System.Spatial.dll"
#r @"Newtonsoft.Json\lib\net45\Newtonsoft.Json.dll"
#r @"Fable.JsonConverter/lib/net45/Fable.JsonConverter.dll"
#endif

open System
open Streamstone
open Microsoft.Azure // Namespace for CloudConfigurationManager
open Microsoft.WindowsAzure.Storage // Namespace for CloudStorageAccount
open Microsoft.WindowsAzure.Storage.Table
open Newtonsoft.Json

let converters : Newtonsoft.Json.JsonConverter array = [|Fable.JsonConverter(); |]

let settings = JsonSerializerSettings (
                        Converters = converters,
                        Formatting = Formatting.Indented,
                        NullValueHandling = NullValueHandling.Ignore)

let serialize obj = JsonConvert.SerializeObject(obj, settings)

let deserialize<'T>(text) = JsonConvert.DeserializeObject<'T>(text, settings)




type Command =
    | CreateLicensee of CreateLicensee
    | AddContact of AddContact
    | ProvideLicense of ProvideLicense
    | ActivateLicense of ActivateLicense
and CreateLicensee = { CompanyName: string; Address: string }
and AddContact = { LicenseeId: Guid; ContactName:string ; Email:string }
and ProvideLicense = { LicenseeId: Guid;  } //ProductCode: Guid
and ActivateLicense = { LicenseeId: Guid; ProductCode: Guid; RegistrationCode: Guid; ActivationDate: DateTime }


type Event =
    | LicenseeCreated of LicenseeCreated
    | ContactAdded of ContactAdded
    | LicenseProvided of LicenseProvided
    | LicenseActivated of LicenseActivated
and LicenseeCreated = { LicenseeId: Guid; CompanyName: string; Address: string }
and ContactAdded = { LicenseeId: Guid; ContactName:string ; Email:string }
and LicenseProvided = { LicenseeId: Guid; ProductCode: Guid } 
and LicenseActivated = { LicenseeId: Guid; ProductCode: Guid; RegistrationCode: Guid; ActivationDate: DateTime }
    


type State = {LicenseeId : Guid option; CurrentContact : string; ProductCode : Guid}
    with 
    static member initial = { LicenseeId = None; CurrentContact = ""; ProductCode = Guid.Empty }
  

 // apply event
let evolve state =
    function
    | LicenseeCreated e -> {state with State.LicenseeId = Some e.LicenseeId }
    | ContactAdded e -> { state with CurrentContact = e.ContactName }
    | LicenseProvided e -> { state with ProductCode = e.ProductCode }
    | LicenseActivated e -> state

let testid = Guid.NewGuid()
let domainEvents = [
    LicenseeCreated { LicenseeId=testid; CompanyName="My Company"; Address="Musterstraße" }
    ContactAdded { LicenseeId=testid; ContactName="My Name"; Email="my@email.de" }
    ContactAdded { LicenseeId=testid; ContactName="Your Name"; Email="my@email.de" }
]

//"replay"
let project = List.fold evolve State.initial
let prepare ()  = 
    let table = CloudStorageAccount.DevelopmentStorageAccount.CreateCloudTableClient().GetTableReference("eventstore")
    table.CreateIfNotExists() |> ignore
    table            
let inline (=>) a b = a, box b
type Data = {Id:Guid; Type:string; Data:string}
let toEventData ev=
    
    let id = Guid.NewGuid()
    let properties = {Id = id;Type = ev.GetType().Name; Data = serialize ev}
    EventData(EventId.From(id), EventProperties.From(properties))


let save (id:Guid) (ev:Event list) =
    let data = ev |> List.map toEventData |> List.toArray
    let table = prepare()
    let partition =  Partition(table , "LicenseeId-" + id.ToString("D"))
    let stream = Stream.TryOpen(partition)
                 |> fun existent -> match existent.Found with
                                    |true -> existent.Stream
                                    |false -> Stream(partition) 
   
   
    printfn "Writing to new stream in partition %A" stream.Partition
    let result = Stream.Write(stream, data)   
    printf "Succesfully written to new stream.\r\nEtag: %s, Version: %i" result.Stream.ETag result.Stream.Version
//handle command
let execute command =
  //  let state:State = project history
    let id, uncommittedEvent = 
        match command with
        | CreateLicensee cmd -> 
            let id = testid//Guid.NewGuid()
            id, LicenseeCreated {LicenseeId = id; CompanyName = cmd.CompanyName; Address = cmd.Address}
        | AddContact cmd -> 
            cmd.LicenseeId, ContactAdded { LicenseeId = cmd.LicenseeId; ContactName = cmd.ContactName ; Email = cmd.Email }
        | ProvideLicense cmd -> 
            cmd.LicenseeId, LicenseProvided { LicenseeId = cmd.LicenseeId; ProductCode = Guid.NewGuid() }
        | ActivateLicense cmd -> 
             cmd.LicenseeId, LicenseActivated { LicenseeId = cmd.LicenseeId; ProductCode = cmd.ProductCode; RegistrationCode = Guid.NewGuid(); ActivationDate = DateTime.UtcNow }
    save id [uncommittedEvent]



//Playground

let domainCommands = [
    CreateLicensee { CompanyName="My Company"; Address="Musterstraße" }
    AddContact { LicenseeId=testid; ContactName="My Name"; Email="my@email.de" }
    AddContact { LicenseeId=testid; ContactName="Your Name"; Email="my@email.de" }
    ProvideLicense { LicenseeId=testid }
]

#time "on"

domainEvents @ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents @ domainEvents@ domainEvents
|> save testid                       

execute (ProvideLicense { LicenseeId=testid })

let table = prepare()
let partition =  Partition(table , "LicenseeId-" + testid.ToString("D"))


type EventEntity(``type``:string, data:string, version:int) =
    new () = EventEntity("", "", 0)
    member val Type = ``type`` with get,set
    member val Data = data with get,set
    member val Version = version with get,set

let s = Stream.Read<EventEntity> (partition, 1, 100000)
s.Events
|> Seq.iter (fun ev -> printfn "%i %s - %s" ev.Version ev.Type ev.Data) 


let toTableEvent data ``type`` =
    let e:Event = deserialize data
//    printfn "%A" e
    e


s.Events
|> Seq.map (fun ev -> toTableEvent ev.Data ev.Type)
|> Seq.toList
|> project 
|> printfn "%A"

// let stateBefore = project domainEvents
// let newDomainEvents = ProvideLicense { LicenseeId=id }
//                       |> execute domainEvents
//                       |> fun e -> List.append domainEvents [e]
// let stateAfter = project newDomainEvents



