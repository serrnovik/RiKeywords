using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace RIKeywords {
    public class MainWindow : Window {
        public MainWindow () {
            AvaloniaXamlLoader.Load (this);
#if DEBUG
            this.AttachDevTools ();
#endif
        }
    }
}