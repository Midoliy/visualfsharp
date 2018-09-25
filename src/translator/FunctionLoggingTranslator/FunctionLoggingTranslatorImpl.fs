namespace FunctionLoggingTranslator

open Microsoft.FSharp.Core.CompilerServices

open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.Ast
open Microsoft.FSharp.Compiler.Configurations
open Microsoft.FSharp.Compiler.ErrorLogger

// ============================================================

module internal Utilities =

    let range0 = Range.range0

    let makeString (str: string) (range: Range.range) : SynExpr =
        SynExpr.Const(SynConst.String(str, range), range)

    let makeLongIdent (name: string) (range: Range.range) : SynExpr =
        let idList =
            name.Split('.')
            |> Seq.map (fun id -> Ident(id, range))
            |> Seq.toList
        let rangeList = List.init (idList.Length - 1) (fun _ -> range)
        let d: SynSimplePatAlternativeIdInfo ref option = None
        SynExpr.LongIdent(false, LongIdentWithDots(idList, rangeList), d, range)

    let stringOfLongIdent (longDotId: LongIdentWithDots) =
        longDotId.Lid
        |> Seq.map (fun id -> id.idText)
        |> String.concat "."

    let insertLogExpr (name: string) (expr: SynExpr) : SynExpr =
        let enterLogExpr =
            SynExpr.App
                (ExprAtomicFlag.Atomic, false, makeLongIdent "System.Diagnostics.Debug.WriteLine" range0
                , SynExpr.Paren(makeString ("Enter function: " + name) range0, range0, Some range0, range0), range0)
        SynExpr.Sequential(SequencePointsAtSeq, true, enterLogExpr, expr, expr.Range)

    // ------------------------------------------------------------

    let traverseBinding = function // SynBinding
        | SynBinding.Binding(accessibility, kind, mustInline, isMutable, attrs, xmlDoc, valData, SynPat.LongIdent(longDotId, li1, li2, li3, li4, li5), returnInfo, expr, range, seqPoint) ->
            let name = stringOfLongIdent longDotId
            SynBinding.Binding(accessibility, kind, mustInline, isMutable, attrs, xmlDoc, valData, SynPat.LongIdent(longDotId, li1, li2, li3, li4, li5), returnInfo, insertLogExpr name expr, range, NoSequencePointAtLetBinding)
        | binding ->
            binding

    let traverseDecl = function // SynModuleDecl
        | SynModuleDecl.Let(b1, bindings, range) ->
            SynModuleDecl.Let(b1, bindings |> List.map traverseBinding, range)
        | decl ->
            decl

    let traverseModule = function // SynModuleOrNamespace
        | SynModuleOrNamespace.SynModuleOrNamespace(longId, isRecursive, isModule, decls, xmlDoc, attribs, accessibility, range) ->
            SynModuleOrNamespace.SynModuleOrNamespace(longId, isRecursive, isModule, decls |> List.map traverseDecl, xmlDoc, attribs, accessibility, range)

// ============================================================

type FunctionLoggingTranslatorImpl() =

    interface ITranslator<ICompilerConfig, ErrorLogger, ParsedInput> with
        member __.Name = "FunctionLoggingTranslator"
        member __.Translate config errorLogger input =
            match input with
            | ParsedInput.ImplFile(ParsedImplFileInput(fileName, isScript, qualifiedNameOfFile, scopedPragmas, hashDirectives, modules, (isLastCompiland, isExe))) ->
                ParsedInput.ImplFile(ParsedImplFileInput(fileName, isScript, qualifiedNameOfFile, scopedPragmas, hashDirectives, modules |> List.map Utilities.traverseModule, (isLastCompiland, isExe)))
            | ParsedInput.SigFile (ParsedSigFileInput(fileName, qualifiedNameOfFile, scopedPragmas, hashDirectives, modules)) ->
                input

[<assembly: Translator(typeof<FunctionLoggingTranslatorImpl>)>]
do ()
