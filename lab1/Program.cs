using lab1.Controller;
using lab1.Model;
using lab1.View;

namespace lab1;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var model = new ExcuseModel();
        var view = new MainForm();
        _ = new MainController(model, view);

        Application.Run(view);
    }
}