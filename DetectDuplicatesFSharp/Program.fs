open System.Text.RegularExpressions
open System.IO

let getDirectory (args:string array) =
    args
    |> Array.tryHead
    |> Option.defaultValue (Directory.GetCurrentDirectory())

let (|Match|_|) regex str =
    let m = Regex(regex).Match(str)
    if m.Success then Some (List.tail [ for x in m.Groups -> x.Value ])
    else None

let findDuplicates file =
    file
    |> File.ReadLines
    |> Seq.choose (
        function
        | Match "<PackageReference Include=\"([^\"]*)\"" [ s ] -> Some s
        | _ -> None)
    |> Seq.groupBy id
    |> Seq.choose (fun (s, x) -> if Seq.length x > 1 then Some s else None)

let processFolder path =
    printfn "Looking for duplicates in %s" path
    let files = Directory.GetFiles(path, "*.csproj", SearchOption.AllDirectories)
    [ for file in files do
        let duplicates = file |> findDuplicates
        if duplicates |> Seq.isEmpty |> not then
            file |> Path.GetFileName, duplicates
    ]

[<EntryPoint>]
let main argv =
    argv
    |> getDirectory 
    |> processFolder
    |> function
       | [] -> printfn "No duplicates found"; 0
       | items -> items |> List.iter (fun item -> printfn "%s -> %A" (fst item) (snd item)); 1
