[<AutoOpen>]
module Domain
open System


type LicenseeId = LicenseeId of Guid 

type Command =
    | CreateLicensee of CreateLicensee
    | AddContact of AddContact
    | ProvideLicense of ProvideLicense
    | ActivateLicense of ActivateLicense
and CreateLicensee = { CompanyName: string; Address: string }
and AddContact = { LicenseeId: LicenseeId; ContactName:string ; Email:string }
and ProvideLicense = { LicenseeId: LicenseeId;  } //ProductCode: Guid
and ActivateLicense = { LicenseeId: LicenseeId; ProductCode: Guid; RegistrationCode: Guid; ActivationDate: DateTime }


type Event =
    | LicenseeCreated of LicenseeCreated
    | ContactAdded of ContactAdded
    | LicenseProvided of LicenseProvided
    | LicenseActivated of LicenseActivated
and LicenseeCreated = { LicenseeId: LicenseeId; CompanyName: string; Address: string }
and ContactAdded = { LicenseeId: LicenseeId; ContactName:string ; Email:string }
and LicenseProvided = { LicenseeId: LicenseeId; ProductCode: Guid } 
and LicenseActivated = { LicenseeId: LicenseeId; ProductCode: Guid; RegistrationCode: Guid; ActivationDate: DateTime }
    


