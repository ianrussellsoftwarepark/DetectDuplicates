open System.IO
open System.Text.RegularExpressions

let processFolder path = seq { 
    for file in Directory.EnumerateFiles(path, "*.csproj", SearchOption.AllDirectories) do
        let packages = seq {
            for line in File.ReadLines(file) do
                let m = Regex("<PackageReference Include=\"([^\"]*)\"").Match(line)
                if m.Success then m.Groups.[1].Value
        }
        for package in packages |> Seq.groupBy id do
            if Seq.length (snd package) = 1 then (file |> Path.GetFileName, fst package)
}

[<EntryPoint>]
let main argv =
    argv
    |> Array.tryHead |> Option.defaultWith Directory.GetCurrentDirectory
    |> fun path -> path |> printfn "Looking for duplicates in %s"; path
    |> processFolder
    |> fun duplicates ->
       if duplicates |> Seq.isEmpty then printfn "No duplicates found"; 0
       else duplicates |> Seq.iter (fun item -> printfn "%s -> %A" (fst item) (snd item)); 1
