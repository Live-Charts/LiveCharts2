// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Runtime.CompilerServices;

#if STRONG_NAMED_ASSEMBLIES
using System.Reflection;

[assembly: AssemblyKeyFile("./../../../LiveCharts.snk")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d53791eaa0d98b405ca858f39169be6af36ceb7a1bca3ca76c6905fd22fddf8c5e4ef2778a5d7a77ad12f08da711fecfc44795c7923739a2acac946b3f1719a6dfc238695bc69cf5d959b3fb6bc4d18d57a97ff8ed897e6b22a6b8155401ee368d77431e74178104b4adca73520b058b9be28d4ec129beb54871778167afa5ce")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.WinForms, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d53791eaa0d98b405ca858f39169be6af36ceb7a1bca3ca76c6905fd22fddf8c5e4ef2778a5d7a77ad12f08da711fecfc44795c7923739a2acac946b3f1719a6dfc238695bc69cf5d959b3fb6bc4d18d57a97ff8ed897e6b22a6b8155401ee368d77431e74178104b4adca73520b058b9be28d4ec129beb54871778167afa5ce")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.WPF, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d53791eaa0d98b405ca858f39169be6af36ceb7a1bca3ca76c6905fd22fddf8c5e4ef2778a5d7a77ad12f08da711fecfc44795c7923739a2acac946b3f1719a6dfc238695bc69cf5d959b3fb6bc4d18d57a97ff8ed897e6b22a6b8155401ee368d77431e74178104b4adca73520b058b9be28d4ec129beb54871778167afa5ce")]
[assembly: InternalsVisibleTo("CoreTests, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d53791eaa0d98b405ca858f39169be6af36ceb7a1bca3ca76c6905fd22fddf8c5e4ef2778a5d7a77ad12f08da711fecfc44795c7923739a2acac946b3f1719a6dfc238695bc69cf5d959b3fb6bc4d18d57a97ff8ed897e6b22a6b8155401ee368d77431e74178104b4adca73520b058b9be28d4ec129beb54871778167afa5ce")]
[assembly: InternalsVisibleTo("LiveChartsCore.BackersPackage, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d53791eaa0d98b405ca858f39169be6af36ceb7a1bca3ca76c6905fd22fddf8c5e4ef2778a5d7a77ad12f08da711fecfc44795c7923739a2acac946b3f1719a6dfc238695bc69cf5d959b3fb6bc4d18d57a97ff8ed897e6b22a6b8155401ee368d77431e74178104b4adca73520b058b9be28d4ec129beb54871778167afa5ce")]

#else

[assembly: InternalsVisibleTo("CoreTests")]
[assembly: InternalsVisibleTo("SnapshotTests")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView")]
[assembly: InternalsVisibleTo("LiveChartsCore.Behaviours")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.WinForms")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.WPF")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.Avalonia")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.WinUI")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.Uno")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.Uno.WinUI")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.XamarinForms")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.Eto")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.Blazor")]
[assembly: InternalsVisibleTo("LiveChartsCore.SkiaSharpView.Maui")]
[assembly: InternalsVisibleTo("LiveChartsCore.BackersPackage")]

#endif

