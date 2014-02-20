namespace DonSymeFactProvider

open System
open System.IO
open System.Reflection
open System.Collections.Generic
open Samples.FSharp.ProvidedTypes
open FSharp.Data
open FSharp.Net

open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations

type js = JsonProvider<"""{ "type": "success", "value": { "id": 268, "joke": "Time waits for no man. Unless that man is Chuck Norris." } }""" >

[<TypeProvider>]
type Provider(config: TypeProviderConfig) as this = 
    inherit TypeProviderForNamespaces()

    let ns = "DonSymeFactProvider"
    let asm = Assembly.GetExecutingAssembly()

    let rnd = Random(DateTime.Now.Millisecond)

    let getFact() =
        try        
            let joke =  js.Parse(Http.Request("http://api.icndb.com/jokes/random"))
            joke.Value.Joke.Replace("Chuck","Don").Replace("Norris","Syme")
        with 
        | ex -> ""

    let createTypes() =        
        let typeDict = Dictionary<string, ProvidedTypeDefinition>()        
        let megaType = ProvidedTypeDefinition("DonsManyFacts",None,HideObjectMethods=true)
         
         
        let rec recusrsiveTypesAreRecursive() =       
            let nextType = ProvidedTypeDefinition(Guid.NewGuid().ToString() + "Don",None,HideObjectMethods=true)            
            nextType.AddMembersDelayed( fun _ ->                
               let nextNextType = recusrsiveTypesAreRecursive()
               megaType.AddMember nextNextType
               let prop = ProvidedProperty("Another true fact about Don!",nextNextType,GetterCode = fun _ -> <@@ obj() @@> )
               prop.AddXmlDocDelayed( fun () -> "<summary>" + getFact() + "</summary>" )
               [prop:>MemberInfo;recusrsiveTypesAreRecursive():>MemberInfo] )
            nextType

        megaType.AddMemberDelayed( fun () ->  
         let nextType = recusrsiveTypesAreRecursive()
         megaType.AddMember(nextType)
         let prop = ProvidedProperty("A true fact about Don!!",nextType,GetterCode = fun _ -> <@@ obj() @@> )
         prop.AddXmlDocDelayed( fun () -> "<summary>" + getFact() + "</summary>" )
         prop)
        megaType
        
    let rootType = ProvidedTypeDefinition(asm, ns, "DonSymeFactProvider", None, HideObjectMethods = true)
    
    do 
      rootType.AddMember(ProvidedConstructor([],InvokeCode = (fun _ -> <@@ obj() @@> )))
      let unicornType = createTypes()
      rootType.AddMember(ProvidedMethod("Create",[],unicornType, InvokeCode = (fun _ -> <@@ obj() @@> ), IsStaticMethod = true))
      rootType.AddMember unicornType
      this.AddNamespace(ns, [rootType])


[<assembly:TypeProviderAssembly>] 
do()