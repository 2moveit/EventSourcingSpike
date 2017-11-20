module SerializationAdapter

open Newtonsoft.Json
open Newtonsoft.Json.Converters


let private converters : Newtonsoft.Json.JsonConverter array = [| Fable.JsonConverter()  |]

let private settings = JsonSerializerSettings ( Converters = converters,
                                                Formatting = Formatting.Indented,
                                                NullValueHandling = NullValueHandling.Ignore)

let serialize obj = JsonConvert.SerializeObject(obj, settings)

let deserialize<'T>(text) = JsonConvert.DeserializeObject<'T>(text, settings)

