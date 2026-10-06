using lab1.Model;

namespace lab1.View;

public sealed class DataForm : Form
{
    private readonly TextBox nameTextBox;
    private readonly RadioButton maleRadioButton;
    private readonly RadioButton femaleRadioButton;

    public event EventHandler<UserData>? SaveRequested;

    public DataForm(UserData data)
    {
        Text = "Ввод данных";
        ClientSize = new Size(360, 135);
        MinimumSize = new Size(360, 135);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 4
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.Controls.Add(new Label
        {
            Text = "Имя:",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 0);

        nameTextBox = new TextBox
        {
            Text = data.Name,
            Dock = DockStyle.Fill
        };
        layout.Controls.Add(nameTextBox, 1, 0);

        layout.Controls.Add(new Label
        {
            Text = "Пол:",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 1);

        var genderPanel = new FlowLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        maleRadioButton = new RadioButton
        {
            Text = "Мужской",
            AutoSize = true,
            Checked = data.Gender == Gender.Male
        };

        femaleRadioButton = new RadioButton
        {
            Text = "Женский",
            AutoSize = true,
            Checked = data.Gender == Gender.Female
        };

        genderPanel.Controls.Add(maleRadioButton);
        genderPanel.Controls.Add(femaleRadioButton);
        layout.Controls.Add(genderPanel, 1, 1);


        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };

        var saveButton = new Button
        {
            Text = "Сохранить",
            AutoSize = true
        };

        var cancelButton = new Button
        {
            Text = "Отмена",
            AutoSize = true,
            DialogResult = DialogResult.Cancel
        };

        saveButton.Click += (_, _) =>
        {
            var dataToSave = new UserData
            {
                Name = nameTextBox.Text,
                Gender = femaleRadioButton.Checked ? Gender.Female : Gender.Male
            };

            SaveRequested?.Invoke(this, dataToSave);
        };

        buttons.Controls.Add(saveButton);
        buttons.Controls.Add(cancelButton);

        layout.Controls.Add(buttons, 0, 3);
        layout.SetColumnSpan(buttons, 2);

        AcceptButton = saveButton;
        CancelButton = cancelButton;

        Controls.Add(layout);
    }

    public void ShowError(string message)
    {
        MessageBox.Show(
            this,
            message,
            "Ошибка",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
