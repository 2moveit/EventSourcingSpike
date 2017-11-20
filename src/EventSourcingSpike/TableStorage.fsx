#r @"..\packages\WindowsAzure.Storage\lib\net45\Microsoft.WindowsAzure.Storage.dll"
#r @"..\packages\Microsoft.WindowsAzure.ConfigurationManager\lib\net40\Microsoft.WindowsAzure.Configuration.dll"
#r @"..\packages\System.Spatial\lib\portable-net45+wp8+win8+wpa\System.Spatial.dll"

open System
open System.IO
open Microsoft.Azure // Namespace for CloudConfigurationManager
open Microsoft.WindowsAzure.Storage // Namespace for CloudStorageAccount
open Microsoft.WindowsAzure.Storage.Table

let storageConnString = "DefaultEndpointsProtocol=https;AccountName=halepbetastorage;AccountKey=jHeOH6g+KRFgFvdwXO4xa4u/+xmAZlfsuZpVlyTf7DROnNmIsKNSonFrEGUKEv0JyGt6SFECo1UnnmT7PfkHNA==;EndpointSuffix=core.windows.net"
let storageAccount = CloudStorageAccount.DevelopmentStorageAccount;// CloudStorageAccount.Parse(storageConnString)
let tableClient = storageAccount.CreateCloudTableClient()

let table = tableClient.GetTableReference("eventstore")
table.CreateIfNotExists()