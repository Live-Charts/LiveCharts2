#if AVALONIA_UI_TESTING
global using Samples = AvaloniaSample;
#endif

#if BLAZOR_UI_TESTING
global using Samples = BlazorSample.Pages;
#endif

#if ETO_UI_TESTING
global using Samples = EtoFormsSample;
#endif

#if MAUI_UI_TESTING
global using Samples = MauiSample;
#endif

#if UNO_UI_TESTING || WINUI_UI_TESTING
global using Samples = WinUISample;
#endif

#if WINFORMS_UI_TESTING
global using Samples = WinFormsSample;
#endif

#if WPF_UI_TESTING
global using Samples = WPFSample;
#endif
