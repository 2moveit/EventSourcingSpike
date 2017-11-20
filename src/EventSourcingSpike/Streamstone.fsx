#if INTERACTIVE
#I @"./../packages"
#r @"Streamstone/lib/netstandard1.6/Streamstone.dll"
#r @"WindowsAzure.Storage\lib\net45\Microsoft.WindowsAzure.Storage.dll"
#r @"Microsoft.WindowsAzure.ConfigurationManager\lib\net40\Microsoft.WindowsAzure.Configuration.dll"
#r @"System.Spatial\lib\portable-net45+wp8+win8+wpa\System.Spatial.dll"
#r @"Newtonsoft.Json\lib\net45\Newtonsoft.Json.dll"
#r @"Fable.JsonConverter/lib/net45/Fable.JsonConverter.dll"
#endif

#load "Domain.fs"
#load "SerializationAdapter.fs"
#load "EventStoreAdapter.fs"
open System
open EventStoreAdapter

// Projection

// state to project
type State1 = {LicenseeId : LicenseeId option; CurrentContact : string; ProductCode : Guid}
    with 
    static member initial = { LicenseeId = None; CurrentContact = ""; ProductCode = Guid.Empty }
  

 // apply event
let evolve1 state1 =
    function
    | LicenseeCreated e -> {state1 with State1.LicenseeId = Some e.LicenseeId }
    | ContactAdded e -> { state1 with CurrentContact = e.ContactName }
    | LicenseProvided e -> { state1 with ProductCode = e.ProductCode }
    | LicenseActivated _ -> state1
  

//"replay"
let project1 = List.fold evolve1 State1.initial

let newLicenseeId() = LicenseeId(Guid.NewGuid())

//handle command
let execute command = async{

    let id, (newEvents:Event list) = 
        match command with
        | CreateLicensee cmd -> 
            let id = newLicenseeId()
            id, [LicenseeCreated {LicenseeId = id; CompanyName = cmd.CompanyName; Address = cmd.Address}]
        | AddContact cmd -> 
              //  let state1:State1 = project1 historyevents
            cmd.LicenseeId, [ContactAdded { LicenseeId = cmd.LicenseeId; ContactName = cmd.ContactName ; Email = cmd.Email }]
        | ProvideLicense cmd -> 
              //  let state2:State2 = project2 historyevents
            cmd.LicenseeId, [LicenseProvided { LicenseeId = cmd.LicenseeId; ProductCode = Guid.NewGuid() }]
        | ActivateLicense cmd -> 
             cmd.LicenseeId, [LicenseActivated { LicenseeId = cmd.LicenseeId; ProductCode = cmd.ProductCode; RegistrationCode = Guid.NewGuid(); ActivationDate = DateTime.UtcNow }]
    do! save id newEvents
    return id, newEvents 
    }


//Playground

#time "on"

let testid = newLicenseeId()
let domainEvents = [
    LicenseeCreated { LicenseeId=testid; CompanyName="My Company"; Address="Musterstraße" }
    ContactAdded { LicenseeId=testid; ContactName="My Name"; Email="my@email.de" }
    ContactAdded { LicenseeId=testid; ContactName="Your Name"; Email="my@email.de" }
]

let domainCommands = [
    CreateLicensee { CompanyName="My Company"; Address="Musterstraße" }
    AddContact { LicenseeId=testid; ContactName="My Name"; Email="my@email.de" }
    AddContact { LicenseeId=testid; ContactName="Your Name"; Email="my@email.de" }
    ProvideLicense { LicenseeId=testid }
]


domainEvents //@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents@ domainEvents @ domainEvents@ domainEvents
|> save testid 
|> Async.RunSynchronously

execute (ProvideLicense { LicenseeId=testid })
|> Async.RunSynchronously

load testid |> Async.RunSynchronously 


let print id = async {
                    let! l = load id 
                    l
                    |> project1 
                    |> printfn "%A"
}

print testid |>  Async.RunSynchronously

// let stateBefore = project domainEvents
// let newDomainEvents = ProvideLicense { LicenseeId=id }
//                       |> execute domainEvents
//                       |> fun e -> List.append domainEvents [e]
// let stateAfter = project newDomainEvents



