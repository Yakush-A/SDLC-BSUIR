using lab1.Model;
using lab1.View;

namespace lab1.Controller;

public sealed class MainController
{
    private readonly ExcuseModel model;
    private readonly MainForm view;

    public MainController(ExcuseModel model, MainForm view)
    {
        this.model = model;
        this.view = view;

        view.EnterDataClicked += OnEnterDataClicked;
        view.GenerateClicked += OnGenerateClicked;
        view.SituationChanged += OnSituationChanged;

        model.StateChanged += OnModelStateChanged;

        view.ShowData(model.Data);
    }

    private void OnEnterDataClicked(object? sender, EventArgs e)
    {
        using var dataForm = new DataForm(model.Data);
        var controller = new DataController(model, dataForm);

        dataForm.ShowDialog(view);

        GC.KeepAlive(controller);
    }

    private void OnGenerateClicked(object? sender, EventArgs e)
    {
        try
        {
            var result = model.GenerateExcuse();
            view.ShowResult(result);
        }
        catch (Exception ex)
        {
            view.ShowError(ex.Message);
        }
    }

    private void OnSituationChanged(object? sender, Situation situation)
    {
        model.SetSituation(situation);
    }

    private void OnModelStateChanged(object? sender, EventArgs e)
    {
        view.ShowData(model.Data);
    }
}

public sealed class DataController
{
    private readonly ExcuseModel model;
    private readonly DataForm view;

    public DataController(ExcuseModel model, DataForm view)
    {
        this.model = model;
        this.view = view;

        view.SaveRequested += OnSaveRequested;
    }

    private void OnSaveRequested(object? sender, UserData data)
    {
        try
        {
            model.SetData(data);
            view.DialogResult = DialogResult.OK;
            view.Close();
        }
        catch (ArgumentException ex)
        {
            view.ShowError(ex.Message);
        }
    }
}
