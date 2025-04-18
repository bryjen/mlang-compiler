﻿namespace Monkey.Parser.Tests.Parser.MonkeyAstParseErrorTests

open Microsoft.CodeAnalysis.Text
open Microsoft.CodeAnalysis.CSharp

open NUnit.Framework

open Monkey.Parser.Errors
open Monkey.Parser.Parser
open Monkey.Parser.Tokenizer

open type Monkey.AST.SyntaxFactory.MonkeySyntaxTokenFactory
open type Monkey.AST.SyntaxFactory.MonkeyExpressionSyntaxFactory
open type Monkey.AST.SyntaxFactory.MonkeyStatementSyntaxFactory


[<AutoOpen>]
module private ErrorTestsHelpers =
    let printErrors (sourceText: SourceText) (errors: ParseError array) =
        let counts = [| 1 .. errors.Length |]
        let filePath = @"C:\Users\admin\Documents\SampleFile.mk"
        for count, error in Array.zip counts errors do
            printfn $"{count}."
            printfn $"{error.GetFormattedMessage(sourceText, Some filePath)}"


[<TestFixture>]
type GenericErrorTesting() =
    
    member this.TestCases : (string * ParseError array) array = [|
        (
            """let foo = 5;
let foobar = 10;
let bar = 20
""",
            [|
                AbsentSemicolonError(TextSpan(0, 0), AbsentSemicolonAt.LetStatement)
            |]
        )
        (
            """let if = 5;""",
            [|
                InvalidVariableNameError(IfKeyword())
            |]
        )
        (
            """let 6pek = 5;""",
            [|
                InvalidVariableNameError(IfKeyword())
            |]
        )
        (
            """let foobar  5;""",
            [|
                AbsentEqualsError(TextSpan(0, 0))
            |]
        )
        (
            """5""",
            [|
                AbsentSemicolonError(TextSpan(0, 0), AbsentSemicolonAt.ExpressionStatement)
            |]
        )
        
        (
            """if 5 > 2) { 5; } else { 10; };""",
            [|
                AbsentOrInvalidTokenError(TextSpan(0, 0), [| SyntaxKind.OpenParenToken |], At.IfExpression)
            |]
        )
        (
            """if (5 > 2 { 5; } else { 10; };""",
            [|
                AbsentOrInvalidTokenError(TextSpan(0, 0), [| SyntaxKind.OpenParenToken |], At.IfExpression)
            |]
        )
        (
            """if (5 > 2)  5; } else { 10; };""",
            [|
                AbsentOrInvalidTokenError(TextSpan(0, 0), [| SyntaxKind.OpenParenToken |], At.IfExpression)
            |]
        )
        (
            """if (5 > 2) { 5;  else { 10; };""",
            [|
                AbsentOrInvalidTokenError(TextSpan(0, 0), [| SyntaxKind.OpenParenToken |], At.IfExpression)
            |]
        )
        (
            """let foobar = fn (Something<int, int} arg1) {};""",
            [|
                AbsentOrInvalidTokenError(TextSpan(0, 0), [| SyntaxKind.OpenParenToken |], At.IfExpression)
            |]
        )
        (
            """let foobar = 5;
            
if (5>2) {
    namespace Monkey;
    using System.Collections.Generic;
};
""",
            [|
                AbsentOrInvalidTokenError(TextSpan(0, 0), [| SyntaxKind.OpenParenToken |], At.IfExpression)
            |]
        )
    |]
    
    [<TestCase(0)>]
    [<TestCase(1)>]
    [<TestCase(2)>]
    [<TestCase(3)>]
    [<TestCase(4)>]
    
    [<TestCase(5)>]
    [<TestCase(6)>]
    [<TestCase(7)>]
    [<TestCase(8)>]
    
    [<TestCase(9)>]
    
    [<TestCase(10)>]
    member this.``Runner``(index: int) =
        let input, expectedErrors = this.TestCases[index]
        let sourceText = SourceText.From(input)
        let tokens = tokenize input 
        
        printfn "Expected Errors:"
        printErrors sourceText expectedErrors
        
        let _, parseErrors = parseTokens tokens
        
        printfn ""
        printfn "Actual Errors:"
        printErrors sourceText parseErrors
        
        Assert.Pass()