module Monkey.Backend.VirtualMachine


open FsToolkit.ErrorHandling
open Monkey.Backend.Code
open Monkey.Backend.Compiler
open Monkey.Backend.Helpers
open Monkey.Frontend.Eval.Object

let private stackSize = 2048
let private trueObj = Object.BooleanType true
let private falseObj = Object.BooleanType false

let private getBoolObj boolValue = match boolValue with | true -> trueObj | false -> falseObj



type VM =
    { Constants: Object array
      Instructions: Instructions
      
      Stack: ObjectWrapper array
      mutable StackPointer: int }
with
    static member FromByteCode(byteCode: Bytecode) =
        { Constants = byteCode.Constants
          Instructions = byteCode.Instructions
          
          Stack = Array.zeroCreate<ObjectWrapper> stackSize
          StackPointer = 0 }
        
    static member private FailedPopMessage = "Could not pop stack. Stack is empty."
        

    member internal this.LastPoppedStackElement() =
        match this.StackPointer with
        | sp when sp >= 0 && sp < stackSize ->
            let peek = this.Stack[sp]
            match System.Object.ReferenceEquals(peek, null) with
            | true -> None 
            | false -> Some peek.Value
        | _ -> None
            
    static member Run(vm: VM) : Result<VM, string> =
        let asByteArr = vm.Instructions.GetBytes()
        let mutable i = 0
        
        // Callback func called whenever a successful instruction slice has been processed
        let callback () = i <- i + 1
        
        let rec runHelper (_vm: VM) : Result<VM, string> =
            if i < asByteArr.Length then
                let opcode = LanguagePrimitives.EnumOfValue<byte, Opcode> asByteArr[i] 
                match opcode with
                | Opcode.OpConstant ->
                    _vm.HandleOpConstant(&i, asByteArr)
                | Opcode.OpAdd | Opcode.OpSub | Opcode.OpMul | Opcode.OpDiv | Opcode.OpEqual | Opcode.OpNotEqual |
                  Opcode.OpGreaterThan ->
                    _vm.HandleInfixOperation(&i, opcode)
                | Opcode.OpTrue | Opcode.OpFalse ->
                    _vm.HandleBooleanOpcode(&i, opcode)
                | Opcode.OpPop ->
                    match _vm.Pop() with
                    | None -> Ok _vm 
                    | Some (newVm, _) -> Ok newVm 
                | _ ->
                    failwith "unrecognized opcode"
                |> Result.map (fun newVm -> callback (); newVm) // Call the callback
                |> Result.bind runHelper
            else
                Ok _vm
            
        runHelper vm
        
        
    member private this.HandleOpConstant(i: byref<int>, byteArr: byte array) : Result<VM, string> =
        let arraySlice = byteArr[i + 1 .. i + 2]
        let constIndex = readUInt16 arraySlice
        i <- i + 2
        this.Push(this.Constants[int constIndex])
        
    member private this.HandleInfixOperation(i: byref<int>, operatorOpcode: Opcode) : Result<VM, string> =
        result {
            let fromOptionToResult opt = if Option.isSome opt then Ok opt.Value else Error VM.FailedPopMessage
            let! newVm, right = this.Pop() |> fromOptionToResult
            let! newVm, left = newVm.Pop() |> fromOptionToResult
            
            let! result =
                match operatorOpcode, left, right with
                // +
                | Opcode.OpAdd, Object.IntegerType l, Object.IntegerType r -> (l + r) |> Object.IntegerType |> Ok 
                | Opcode.OpAdd, Object.StringType l, Object.StringType r -> failwith "todo"
                | Opcode.OpAdd, Object.IntegerType l, Object.StringType r -> failwith "todo"
                | Opcode.OpAdd, Object.StringType l, Object.IntegerType r -> failwith "todo"
                
                // -
                | Opcode.OpSub, Object.IntegerType l, Object.IntegerType r -> (l - r) |> Object.IntegerType |> Ok
                
                // *
                | Opcode.OpMul, Object.IntegerType l, Object.IntegerType r -> (l * r) |> Object.IntegerType |> Ok
                
                // /
                | Opcode.OpDiv, Object.IntegerType l, Object.IntegerType r -> (l / r) |> Object.IntegerType |> Ok
                
                // ==
                | Opcode.OpEqual, Object.IntegerType l, Object.IntegerType r -> l = r |> getBoolObj |> Ok 
                | Opcode.OpEqual, Object.BooleanType l, Object.BooleanType r -> l = r |> getBoolObj |> Ok
                
                // !=
                | Opcode.OpNotEqual, Object.IntegerType l, Object.IntegerType r -> l <> r |> getBoolObj |> Ok 
                | Opcode.OpNotEqual, Object.BooleanType l, Object.BooleanType r -> l <> r |> getBoolObj |> Ok
                
                // >
                | Opcode.OpGreaterThan, Object.IntegerType l, Object.IntegerType r -> l > r |> getBoolObj |> Ok 
                
                | _ -> Error $"The operation left: \"{left.Type()}\" right: \"{right.Type()}\" opcode: {operatorOpcode} is not valid."
            
            return! newVm.Push(result)
        }
        
    member private this.HandleBooleanOpcode(i: byref<int>, booleanOpcode: Opcode) : Result<VM, string> =
        match booleanOpcode with
        | Opcode.OpTrue -> this.Push(trueObj) 
        | Opcode.OpFalse -> this.Push(falseObj) 
        | _ -> Error $"Fatal: The \"{booleanOpcode}\" does not represent a boolean."
        
    member private this.Push(object: Object) : Result<VM, string> =
        match this.StackPointer >= stackSize with
        | true ->
            Error "Stack Overflow"
        | false ->
            this.Stack[this.StackPointer] <- ObjectWrapper object
            this.StackPointer <- this.StackPointer + 1
            Ok this
            
    member private this.Pop() : (VM * Object) option =
        match this.StackPointer >= stackSize with
        | true -> None
        | false ->
            let i = this.StackPointer - 1
            let objectWrapper = this.Stack[i]
            // this.Stack[i] <- null
            
            match isNull objectWrapper with
            | true -> None 
            | false ->
                this.StackPointer <- this.StackPointer - 1
                Some (this, objectWrapper.Value) 
