using System;
using Eto.Forms;
using Eto.Drawing;

namespace EtoFormsSample;

public class Form1 : Form
{
    private readonly DynamicLayout _leftside = new();
    private readonly DynamicLayout _layout = new();

    public Form1()
    {
        Title = $"Eto.Forms Sample ({Eto.Platform.Instance} / {System.Runtime.InteropServices.RuntimeInformation.OSDescription})";

        ClientSize = new Size(3, 2) * 300;

        var listbox = new ListBox() { DataStore = ViewModelsSamples.Index.Samples };
        listbox.SelectedIndexChanged += listBox1_SelectedIndexChanged;

        //var image = new ImageView() { Image = Bitmap.FromResource("EtoFormsSample.Images.livecharts.png") };

        _leftside = new DynamicLayout(
            //new DynamicRow(image),
            new DynamicRow(listbox)
            );

        _ = _layout.AddRow(_leftside, new Panel());

        Content = _layout;
    }

    private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        var selected = (sender as ListBox).SelectedKey;
        var chart = (Control)Activator.CreateInstance(null, $"EtoFormsSample.{selected.Replace('/', '.')}.View").Unwrap();

        var layout = new DynamicLayout();
        _ = layout.AddRow(_leftside, chart);
        Content = layout;
    }
}
