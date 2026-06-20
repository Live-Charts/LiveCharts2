# Consumer prerequisite: wasm-tools workload

SkiaSharp.Views.Blazor must be linked into the consumer's WebAssembly
bundle so its emscripten `Module` is exposed as `globalThis.SkiaSharpModule`
via `SkiaSharpInterop.js`. That link step requires the .NET WebAssembly
build tools workload — without it, the first chart render throws
`ReferenceError: Module is not defined` (see #2229).

Install it:

```bash
dotnet workload install wasm-tools
```

.NET workloads are scoped to the SDK feature band, so a .NET SDK update — even a
minor one that bumps the feature band (e.g. `10.0.1xx` → `10.0.2xx`) — can leave
the workload missing again and re-trigger `LVC0001` on the next build. After
updating the SDK, re-run the command above or `dotnet workload update`.

The package emits build error `LVC0001` if the workload is missing, so
consumers see the requirement at build time instead of debugging the
runtime crash. The check lives in
`build/LiveChartsCore.SkiaSharpView.Blazor.targets` and is packed into
`buildTransitive/` so direct and transitive consumers both get it.

# About *.ts

If the domInterop.ts changes, the *.js files must be manually updated, this due a possible bug where the
*.js files were not consistently generated on the MSBuild process, by the Microsoft.TypeScript.MSBuild package.

I am not completely sure if this is a bug on the MSBuild, the Microsoft.TypeScript.MSBuild the or
if it is a bug on the way I am using it.

Since the domInterop.ts barely changes, let's keep it simple and update the *.js files manually.

Calling `tsc` on the terminal will generate the *.js files, typescript must be installed globally,
`npm install -g typescript`.
