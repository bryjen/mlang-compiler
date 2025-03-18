﻿module rec Monkey.Frontend.CLR.Syntax.AstTraverser

open Microsoft.CodeAnalysis.CSharp
open Monkey.Frontend.CLR.Syntax.Ast


let printMonkeySyntaxNodeTree (monkeySyntaxNode: MonkeySyntaxNode) =
    let indentation = 0
    match monkeySyntaxNode with
    | CompilationUnitSyntax -> failwith "todo"
    | UsingDeclarationSyntax -> failwith "todo"
    | ArgumentListSyntax argumentListSyntax -> onArgumentListSyntax indentation argumentListSyntax
    | ParameterListSyntax parameterListSyntax -> onParameterListSyntax indentation parameterListSyntax
    | ExpressionSyntax expressionSyntax -> onExpressionSyntax indentation expressionSyntax
    | StatementSyntax statementSyntax -> onStatementSyntax indentation statementSyntax
    
let private indentationStr = "    "
        
let private onSyntaxToken (indentation: int) (syntaxToken: SyntaxToken) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(SyntaxToken)) (syntaxToken.ToString() |> normalizeString)
    onSyntaxKind (indentation + 1) syntaxToken.Kind
    
    if syntaxToken.Value.IsSome then
        let value = syntaxToken.Value.Value
        let indentation = indentation + 1
        printfn "%svalue type : %s" (String.replicate indentation indentationStr) (value.GetType().ToString())
        printfn "%svalue : %s" (String.replicate indentation indentationStr) (value.ToString())
        
    
let private onSyntaxKind (indentation: int) (syntaxKind: SyntaxKind) =
    printfn "%sKind : %s" (String.replicate indentation indentationStr) (syntaxKind.ToString()) 
    
   
    
let private normalizeString (str: string) =
    let normalziedStr = str.Replace("\n", "\\n").Trim()
    $"`{normalziedStr}`"
    
    
let private onArgumentListSyntax (indentation: int) (argumentListSyntax: ArgumentListSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(ArgumentListSyntax)) (argumentListSyntax.ToString() |> normalizeString)
    argumentListSyntax.ArgumentSyntax |> Array.iter (onExpressionSyntax (indentation + 1))
    
let private onParameterListSyntax (indentation: int) (parameterListSyntax: ParameterListSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(ParameterListSyntax)) (parameterListSyntax.ToString() |> normalizeString)
    // parameterListSyntax.ParameterSyntax |> Array.iter (onParameterListSyntax (indentation + 1))
    // TODO: onParameterSyntax
    
    
let rec private onExpressionSyntax (indentation: int) (expressionSyntax: ExpressionSyntax) =
    match expressionSyntax with
    | ParenthesizedExpressionSyntax parenthesizedExpressionSyntax -> onParenthesizedExpressionSyntax indentation parenthesizedExpressionSyntax
    | FunctionExpressionSyntax functionExpressionSyntax -> onFunctionExpressionSyntax indentation functionExpressionSyntax
    | BinaryExpressionSyntax binaryExpressionSyntax -> onBinaryExpressionSyntax indentation binaryExpressionSyntax
    | InterpolatedStringExpressionSyntax interpolatedStringExpressionSyntax -> onInterpolatedStringExpressionSyntax indentation interpolatedStringExpressionSyntax
    | InvocationExpressionSyntax invocationExpressionSyntax -> onInvocationExpressionSyntax indentation invocationExpressionSyntax
    | LiteralExpressionSyntax literalExpressionSyntax -> onLiteralExpressionSyntax indentation literalExpressionSyntax
    | PostfixExpressionSyntax postfixExpressionSyntax -> onPostfixExpressionSyntax indentation postfixExpressionSyntax
    | PrefixExpressionSyntax prefixExpressionSyntax -> onPrefixExpressionSyntax indentation prefixExpressionSyntax
    | IdentifierNameSyntax identifierNameSyntax -> onIdentifierNameSyntax indentation identifierNameSyntax
    | TypeSyntax typeSyntax -> onTypeSyntax indentation typeSyntax

and private onParenthesizedExpressionSyntax (indentation: int) (parenthesizedExpressionSyntax: ParenthesizedExpressionSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(ParenthesizedExpressionSyntax)) (parenthesizedExpressionSyntax.ToString() |> normalizeString)
    onSyntaxToken (indentation + 1) parenthesizedExpressionSyntax.OpenParenToken
    onExpressionSyntax (indentation + 1) parenthesizedExpressionSyntax.Expression
    onSyntaxToken (indentation + 1) parenthesizedExpressionSyntax.CloseParenToken
    
and private onFunctionExpressionSyntax (indentation: int) (functionExpressionSyntax: FunctionExpressionSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(FunctionExpressionSyntax)) (functionExpressionSyntax.ToString() |> normalizeString)
    onParameterListSyntax (indentation + 1) functionExpressionSyntax.ParameterList
    onBlockSyntax (indentation + 1) functionExpressionSyntax.Body
    onTypeSyntax (indentation + 1) functionExpressionSyntax.ReturnType
    
and private onBinaryExpressionSyntax (indentation: int) (binaryExpressionSyntax: BinaryExpressionSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(BinaryExpressionSyntax)) (binaryExpressionSyntax.ToString() |> normalizeString)
    onExpressionSyntax (indentation + 1) binaryExpressionSyntax.Left
    onExpressionSyntax (indentation + 1) binaryExpressionSyntax.Right
    
and private onInterpolatedStringExpressionSyntax (indentation: int) (interpolatedStringExpressionSyntax: InterpolatedStringExpressionSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(InterpolatedStringExpressionSyntax)) (interpolatedStringExpressionSyntax.ToString() |> normalizeString)
    // TODO: interpolatedStringExpressionSyntax.Contents
    
and private onInvocationExpressionSyntax (indentation: int) (invocationExpressionSyntax: InvocationExpressionSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(InvocationExpressionSyntax)) (invocationExpressionSyntax.ToString() |> normalizeString)
    onExpressionSyntax (indentation + 1) invocationExpressionSyntax.LeftExpression
    onArgumentListSyntax (indentation + 1) invocationExpressionSyntax.Arguments
    
and private onLiteralExpressionSyntax (indentation: int) (literalExpressionSyntax: LiteralExpressionSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(LiteralExpressionSyntax)) (literalExpressionSyntax.ToString() |> normalizeString)
    onSyntaxKind (indentation + 1) literalExpressionSyntax.Kind
    onSyntaxToken (indentation + 1) literalExpressionSyntax.Token
    
and private onPostfixExpressionSyntax (indentation: int) (postfixExpressionSyntax: PostfixExpressionSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(PostfixExpressionSyntax)) (postfixExpressionSyntax.ToString() |> normalizeString)
    onSyntaxKind (indentation + 1) postfixExpressionSyntax.Kind
    onExpressionSyntax (indentation + 1) postfixExpressionSyntax.Operand
    onSyntaxToken (indentation + 1) postfixExpressionSyntax.OperatorToken
    
and private onPrefixExpressionSyntax (indentation: int) (prefixExpressionSyntax: PrefixExpressionSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(PrefixExpressionSyntax)) (prefixExpressionSyntax.ToString() |> normalizeString)
    onSyntaxKind (indentation + 1) prefixExpressionSyntax.Kind
    onSyntaxToken (indentation + 1) prefixExpressionSyntax.OperatorToken
    onExpressionSyntax (indentation + 1) prefixExpressionSyntax.Operand
    
and private onIdentifierNameSyntax (indentation: int) (identifierNameSyntax: IdentifierNameSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(IdentifierNameSyntax)) (identifierNameSyntax.ToString() |> normalizeString)
    
    
and private onTypeSyntax (indentation: int) (typeSyntax: TypeSyntax) =
    match typeSyntax with
    | NameSyntax nameSyntax -> onNameSyntax indentation nameSyntax
    | BuiltinTypeSyntax builtinTypeSyntax -> onBuiltinTypeSyntax indentation builtinTypeSyntax
    | FunctionTypeSyntax functionTypeSyntax -> onFunctionTypeSyntax indentation functionTypeSyntax
    
and private onNameSyntax (indentation: int) (nameSyntax: NameSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(NameSyntax)) (nameSyntax.ToString() |> normalizeString)
    
and private onBuiltinTypeSyntax (indentation: int) (builtinTypeSyntax: BuiltinTypeSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(BuiltinTypeSyntax)) (builtinTypeSyntax.ToString() |> normalizeString)
    
and private onFunctionTypeSyntax (indentation: int) (functionTypeSyntax: FunctionTypeSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(FunctionTypeSyntax)) (functionTypeSyntax.ToString() |> normalizeString)
    
    
    
let rec private onStatementSyntax (indentation: int) (statementSyntax: StatementSyntax) =
    match statementSyntax with
    | BlockSyntax blockSyntax -> onBlockSyntax indentation blockSyntax
    | ExpressionStatementSyntax expressionStatementSyntax -> onExpressionStatementSyntax indentation expressionStatementSyntax
    | IfStatementSyntax ifStatementSyntax -> onIfStatementSyntax indentation ifStatementSyntax
    | VariableDeclarationStatementSyntax variableDeclarationStatementSyntax -> onVariableDeclarationStatementSyntax indentation variableDeclarationStatementSyntax
    
and private onBlockSyntax (indentation: int) (blockSyntax: BlockSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(BlockSyntax)) (blockSyntax.ToString() |> normalizeString)
    blockSyntax.Statements |> Array.iter (onStatementSyntax (indentation + 1))
    
and private onExpressionStatementSyntax (indentation: int) (expressionStatementSyntax: ExpressionStatementSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(ExpressionStatementSyntax)) (expressionStatementSyntax.ToString() |> normalizeString)
    onExpressionSyntax (indentation + 1) expressionStatementSyntax.Expression 
    
    
and private onIfStatementSyntax (indentation: int) (ifStatementSyntax: IfStatementSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(IfStatementSyntax)) (ifStatementSyntax.ToString() |> normalizeString)
    onExpressionSyntax (indentation + 1) ifStatementSyntax.Condition
    onBlockSyntax (indentation + 1) ifStatementSyntax.Clause
    
    ifStatementSyntax.ElseIfClauses |> Array.iter (onElseIfClauseSyntax (indentation + 1))
    
    if ifStatementSyntax.ElseClause.IsSome then
        onElseClauseSyntax (indentation + 1) ifStatementSyntax.ElseClause.Value
    
and private onElseIfClauseSyntax (indentation: int) (elseIfClauseSyntax: ElseIfClauseSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(ElseIfClauseSyntax)) (elseIfClauseSyntax.ToString() |> normalizeString)
    onExpressionSyntax (indentation + 1) elseIfClauseSyntax.Condition
    onBlockSyntax (indentation + 1) elseIfClauseSyntax.Clause
    
and private onElseClauseSyntax (indentation: int) (elseClauseSyntax: ElseClauseSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(ElseClauseSyntax)) (elseClauseSyntax.ToString() |> normalizeString)
    onBlockSyntax (indentation + 1) elseClauseSyntax.ElseClause
    
    
and private onVariableDeclarationStatementSyntax (indentation: int) (variableDeclarationStatementSyntax: VariableDeclarationStatementSyntax) =
    printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(VariableDeclarationStatementSyntax)) (variableDeclarationStatementSyntax.ToString() |> normalizeString)
    
    if variableDeclarationStatementSyntax.TypeAnnotation.IsSome then
        printfn "%s%s : %s" (String.replicate indentation indentationStr) (nameof(VariableTypeAnnotation)) (variableDeclarationStatementSyntax.TypeAnnotation.Value.ToString() |> normalizeString)
