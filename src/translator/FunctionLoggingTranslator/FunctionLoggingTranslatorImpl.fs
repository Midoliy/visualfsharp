namespace FunctionLoggingTranslator

open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Compiler.Configurations
open Microsoft.FSharp.Compiler.Ast

type FunctionLoggingTranslatorImpl() = 
    interface ITranslator<ICompilerConfig, ParsedInput> with
        member __.Name = "FunctionLoggingTranslator"
        member __.Translate config input =
            printfn "FunctionLoggingTranslator: HelloTranslator."
            printfn "%A\n" input
            input

[<assembly: Translator(typeof<FunctionLoggingTranslatorImpl>)>]
do ()
