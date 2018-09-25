
let addFunc a b =
    a + b
let subFunc a b =
    a - b

[<EntryPoint>]
let main argv =
    let r1 = addFunc 1 2
    let r2 = subFunc 1 2
    printfn "addFunc=%d, subFunc=%d" r1 r2
    0
