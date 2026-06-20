# About docs in this repository

The docs in this repo use markdown and [Scriban](https://github.com/scriban/scriban) to preprocess the md files,
there is a closed source tool that does the prerocessing, there is nothing special about the tool, it just uses Scriban
on all the files in the docs folder. Scriban is a templating language similar to Liquid, and the tool just replaces the
templates with the actual content.

Scriban will replace {{ variable }} with the actual value, or run a function {{ function_name params }}
with the result of the function.

## Available variables

| Variable               | Description                                                                                     |
|------------------------|-------------------------------------------------------------------------------------------------|
| version                | The current version of LiveCharts2, e.g. 2.0.0-beta.1                                           |
| platform               | The current platform, e.g. wpf, winforms, avalonia, maui, uno                                   |
| platforn_display_name  | The platform name but as shown in NuGet, (WPF, Blazor, Maui, Uno.WinUI, Avalonia, WinUI, Eto)   |
| platform_samples_folder| The folder name for the current platform samples in the repo                                    |
| view_extension         | Resolves the view file depending on the platform, e.g. "/View.xaml" for wpf, "/View.axaml" for avalonia or ".razor" for Blazor |
| view_code              | Resolves the view code depending on the platform, e.g. "View.xaml.cs" for wpf, "View.axaml.cs" for avalonia or ".razor" for Blazor |
| avalonia               | true if the current platform is Avalonia, false otherwise                                       |
| blazor                 | true if the current platform is Blazor, false otherwise                                         |
| maui                   | true if the current platform is .NET MAUI, false otherwise                                      |
| winforms               | true if the current platform is WinForms, false otherwise                                       |
| wpf                    | true if the current platform is WPF, false otherwise                                            |
| winui                  | true if the current platform is WinUI, false otherwise                                          |
| uno                    | true if the current platform is Uno, false otherwise                                            |
| eto                    | true if the current platform is Eto.Forms, false otherwise                                      |
| desktop                | true if the current platform is a desktop platform (WPF, WinForms, Avalonia, Uno, WinUI, Eto)   |
| mobile                 | true if the current platform is a mobile platform (Maui, Uno, Avalonia), false otherwise        |
| mvvm                   | true if the current sample uses MVVM, false otherwise                                           |
| xaml                   | true if the current platform uses XAML (WPF, WinUI, WinForms, Avalonia, Uno), false otherwise   |
| xaml2006               | true if the current platform uses XAML 2006 (WPF and WinUI), false otherwise                    |
| full_name              | The full name of the current sample, e.g. LiveChartsCore.SkiaSharpView.WPF.Samples.Axes.Basic   |
| view_title             | if xaml then "XAML", if blazor then "Razor" otherwise "Code behind"                             |
| view                   | The code of the view file of the current sample                                                 |

## Available functions

| Function                            | Description                                                                        |
|-------------------------------------|------------------------------------------------------------------------------------|
| render "path/to/file.md"            | Renders the content of the specified file.                                         |
| render_current_directory_view       | Simiar to render but uses the sample directory as parameter and gets the view file |
| render_current_directory_view_model | Simiar to render but uses the sample directory as parameter and gets the viewmodel |
