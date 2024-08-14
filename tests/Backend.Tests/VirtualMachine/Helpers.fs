[<AutoOpen>]
module Monkey.Backend.Tests.VirtualMachine.Helpers

open FsToolkit.ErrorHandling
open Monkey.Frontend.Eval.Object

[<RequireQualifiedAccess>]
module VMHelpers =
    
    let assertObjectIsNullType (actual: Object) =
        match actual with
        | NullType -> Ok ()
        | _ -> Error $"'actual' is not a 'NullType', got '{actual.Type()}'"
    
    let private testIntegerObject (expected: int64) (actual: Object) =
        result {
            let! asIntegerType = 
                match actual with
                | Object.IntegerType intType -> Ok intType
                | _ -> Error $"'actual' is not an 'IntegerType', got '{actual.Type()}'."
            
            return! 
                match asIntegerType = expected with
                | true -> Ok () 
                | false -> Error $"'actual' has wrong value, expected {expected}, but got {asIntegerType}."
        }
        
    let private testBooleanObject (expected: bool) (actual: Object) =
        result {
            let! boolValue =
                match actual with
                | Object.BooleanType boolType -> Ok boolType
                | _ -> Error $"'actual' is not an 'BooleanType', got '{actual.Type()}'."
                
            return! 
                match boolValue = expected with
                | true -> Ok () 
                | false -> Error $"'actual' has wrong value, expected {expected}, but got {boolValue}."
        }
        
    let private testStringObject (expected: string) (actual: Object) =
        result {
            let! stringValue =
                match actual with
                | Object.StringType stringValue -> Ok stringValue 
                | _ -> Error $"'actual' is not an 'StringType', got '{actual.Type()}'."
                
            return! 
                match stringValue = expected with
                | true -> Ok () 
                | false -> Error $"'actual' has wrong value, expected \"{expected}\", but got \"{stringValue}\"."
        }
        
    let testExpectedObject (expected: obj) (actual: Object) =
        match expected with
        | null -> assertObjectIsNullType actual
        | :? int64 as value -> testIntegerObject value actual 
        | :? int as value -> testIntegerObject (int64 value) actual 
        | :? bool as value -> testBooleanObject value actual 
        | :? string as value -> testStringObject value actual 
        | _ -> failwith $"Test method does not handle expected type \"{expected.GetType()}\""
