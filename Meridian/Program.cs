using System;
using System.Collections.Generic;
using System.Text;

namespace Meridian
{
    public class Program
    {
        [System.STAThread()]
        public static void Main()
        {
            using (new Meridian.WrappedControls.App())
            {
                var app = new App();
                app.InitializeComponent();
                app.Run();
            }
        }
    }
}
