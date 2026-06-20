#pragma warning disable IDE0073 // The file header is missing or not located at the top of the file
#pragma warning disable format

using Factos.Server;
using Factos.Server.Settings;
using Factos.Server.Settings.Apps;
using Microsoft.Testing.Extensions;
using Microsoft.Testing.Platform.Builder;

// About UI test:
// we use https://github.com/beto-rodriguez/Factos
// it allows us to run the same UI tests against multiple UI frameworks,
// including desktop, web and mobile platforms.

// how it works:
// we define tests in the SharedUITests project,
// then each sample app references the SharedUITests project,
// finally factos starts each sample app, connects to it and runs the tests defined in SharedUITests.

#if DEBUG
// notice on Debug the cli args are overridden to make it easier to test locally,
// on Release the args passed from the CI pipeline are used.

// To select which app to run, set the appToRun variable, options are listed at the toTest table below (uid column)
// setting appToRun to "manual-start" will wait for you to start the app manually.

// uno and maui require also to pass the target framework, see tf variable below, will be ignored if not required by the tested app.
// emulators must be running before starting the tests.

var appToRun = "maui";
var tf = "net10.0-windows10.0.19041.0";

args = [
    "--select", appToRun,
    "--test-env", $"tf={tf}"
];

// in debug we use the relative path from the bin\debug folder to the samples
var root = "../../../../../samples";
#else
// in release we use the relative path from the root of the repo for CI purposes
var root = "samples";
#endif

var testsBuilder = await TestApplication.CreateBuilderAsync(args);
var testedApps = new List<TestedApp>();

MSBuildArg[] msBuildArgs = [
    new("UITesting", "true")
];

#if !DEBUG
// in CI we use the nuget packages for everything
// we pack and test the nuget packages against the samples
// lvcversionsuffix is a version suffix we pass from the pipeline, it can be something like "-ci-1234"
// where 1234 is a build number or a commit id which NuGet packages were published with.
msBuildArgs = [
    .. msBuildArgs,
    new("UseNuGetForSamples", "true"),
    new("LiveChartsVersionSuffix", "[lvcversionsuffix]")
];
#endif

MSBuildArg tf_var = new("TargetFramework", "[tf]");
MSBuildArg tf_n10w = new("TestBuildTargetFramework", "net10.0-windows");
MSBuildArg tf_n10w10 = new("TestBuildTargetFramework", "net10.0-windows10.0.19041.0");
MSBuildArg tf_n462 = new("TestBuildTargetFramework", "net462");
MSBuildArg isTest = new("IsTestBuild", "true");
MSBuildArg[] iphoneBuild = [
    ..msBuildArgs,
    // iOS SDK 26.2 renamed the device-selection MSBuild property from `_DeviceName`
    // to `Device` (see Microsoft.Sdk.Mobile.targets: `DeviceName="$(Device)"`).
    // Without it, mlaunch defaults to "iPhone 4s" (MT1207). With it, the
    // `:v2:udid=<UDID>` mlaunch syntax still works and is required.
    new("Device", "[device]"),
    new("MTouchUseLlvm", "false"),
    new("MtouchLink", "SdkOnly"),
    new("RunAOTCompilation", "false"),
    new("UseInterpreter", "true")
];
MSBuildArg[] winUIArgs = [
    .. msBuildArgs,
    new("RuntimeIdentifier", "win-x64"),
    new("WindowsPackageType", "None"),
    new("WindowsAppSDKSelfContained", "true")
];

const string avaloniaDir = "AvaloniaSample/Platforms/AvaloniaSample";
const string unoDir = "UnoPlatformSample/UnoPlatformSample";

TestRecord[] toTest = [
    // | projectPath                           | uid                    | msBuildArgs                           | host
    // ---------------------------------------------------------------------------------------------------------------------------------------
    new($"{root}/{avaloniaDir}.Android",        "avalonia-android",     msBuildArgs),
    new($"{root}/{avaloniaDir}.Browser",        "avalonia-browser",     msBuildArgs,                            AppHost.HeadlessChrome),
    new($"{root}/{avaloniaDir}.Desktop",        "avalonia-desktop",     msBuildArgs),
    new($"{root}/{avaloniaDir}.iOS",            "avalonia-ios",         [..iphoneBuild, new("RuntimeIdentifier", "iossimulator-arm64")]),

    new($"{root}/BlazorSample",                 "blazor",               msBuildArgs,                            AppHost.HeadlessChrome),

    new($"{root}/MauiSample",                   "maui",                 [..msBuildArgs, tf_var]),
    new($"{root}/MauiSample",                   "maui-ios",             [..iphoneBuild, tf_var, new("RuntimeIdentifier", "iossimulator-arm64")]),

    new($"{root}/{unoDir}",                     "uno",                  [..msBuildArgs, tf_var]),
    // browserwasm needs a browser host: AppHost.Auto only `dotnet run`s the dev
    // server and nothing ever loads the page, so the app never connects and the
    // run idles until the connection timeout — the reason the uno browser CI job
    // was disabled. A separate uid because the `uno` record is shared by the
    // desktop/windows/android target frameworks, which need the Auto host.
    new($"{root}/{unoDir}",                     "uno-browser",          [..msBuildArgs, tf_var],                AppHost.HeadlessChrome),
    // same app on Uno's NATIVE renderer (no SkiaRenderer UnoFeature) — the
    // #2020/#2333 scenario, charts render through SKXamlCanvas instead of
    // SKCanvasElement. Needs a clean bin/obj when alternating with uno-browser
    // locally; stale Uno assets from the other renderer break the app at runtime.
    new($"{root}/{unoDir}",                     "uno-browser-native",   [..msBuildArgs, tf_var, new("LvcUnoNativeRenderer", "true")], AppHost.HeadlessChrome),

    new($"{root}/WinUISample/WinUISample",      "winui",                winUIArgs),
    new($"{root}/EtoFormsSample",               "eto",                  msBuildArgs),

    // for winforms and wpf we ensure the nuget packages work on multiple target frameworks
    // because net framework uses strong named assemblies
    // there is also https://github.com/mono/SkiaSharp/issues/3153, which could cause conflicts when the package is restored

    new($"{root}/WinFormsSample",               "winforms-net10",       [..msBuildArgs, tf_n10w, isTest]),
    new($"{root}/WinFormsSample",               "winforms-net10w19041", [..msBuildArgs, tf_n10w10, isTest]),
    new($"{root}/WinFormsSample",               "winforms-net462",      [..msBuildArgs, tf_n462, isTest]),

    new($"{root}/WpfSample",                    "wpf-net10",            [..msBuildArgs, tf_n10w, isTest]),
    new($"{root}/WpfSample",                    "wpf-net10w19041",      [..msBuildArgs, tf_n10w10, isTest]),
    new($"{root}/WpfSample",                    "wpf-net462",           [..msBuildArgs, tf_n462, isTest])
];

testedApps
    .AddManuallyStartedApp();

foreach (var testRecord in toTest)
    testedApps.Add(
        project:        testRecord.projectPath,
        uid:            testRecord.Uid,
        msBuildArgs:    testRecord.MSBuildArgs,
        appHost:        testRecord.host);

testsBuilder
    .AddFactos(new FactosSettings()
    {
        ConnectionTimeout = 300,
        TestedApps = testedApps
    })
    .AddTrxReportProvider();

using var testApp = await testsBuilder.BuildAsync();

return await testApp.RunAsync();

public record TestRecord(
    string projectPath, string Uid, MSBuildArg[]? MSBuildArgs = null, AppHost host = AppHost.Auto);
