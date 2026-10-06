using lab1.Model;

namespace lab1.View;

public sealed class MainForm : Form
{
    private readonly ComboBox situationComboBox;
    private readonly Label nameValueLabel;
    private readonly Label genderValueLabel;
    private readonly TextBox resultTextBox;
    private readonly Button enterDataButton;
    private readonly Button generateButton;

    public event EventHandler? EnterDataClicked;
    public event EventHandler? GenerateClicked;
    public event EventHandler<Situation>? SituationChanged;

    public Situation SelectedSituation =>
        situationComboBox.SelectedItem is SituationItem item
            ? item.Value
            : Situation.Study;

    public MainForm()
    {
        Text = "Генератор оправданий";
        MinimumSize = new Size(620, 430);
        StartPosition = FormStartPosition.CenterScreen;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            ColumnCount = 2,
            RowCount = 8
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label
        {
            Text = "Ситуация:",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(3, 8, 15, 3)
        }, 0, 0);

        situationComboBox = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Left,
            Width = 220
        };

        situationComboBox.Items.AddRange(
        [
            new SituationItem(Situation.Study, "Учёба"),
            new SituationItem(Situation.Work, "Работа"),
            new SituationItem(Situation.Lateness, "Опоздание"),
            new SituationItem(Situation.Forgetfulness, "Забывчивость")
        ]);
        situationComboBox.SelectedIndex = 0;
        situationComboBox.SelectedIndexChanged += (_, _) =>
            SituationChanged?.Invoke(this, SelectedSituation);

        layout.Controls.Add(situationComboBox, 1, 0);

        layout.Controls.Add(new Label
        {
            Text = "Введённые данные:",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(3, 15, 3, 5)
        }, 0, 1);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 1)!, 2);

        layout.Controls.Add(new Label
        {
            Text = "Имя:",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 2);

        nameValueLabel = new Label
        {
            Text = "—",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        };
        layout.Controls.Add(nameValueLabel, 1, 2);

        layout.Controls.Add(new Label
        {
            Text = "Пол:",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        }, 0, 3);

        genderValueLabel = new Label
        {
            Text = "—",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        };
        layout.Controls.Add(genderValueLabel, 1, 3);




        enterDataButton = new Button
        {
            Text = "Ввести данные",
            AutoSize = true,
            Padding = new Padding(10, 5, 10, 5)
        };
        enterDataButton.Click += (_, _) =>
            EnterDataClicked?.Invoke(this, EventArgs.Empty);

        layout.Controls.Add(enterDataButton, 0, 5);
        layout.SetColumnSpan(enterDataButton, 2);

        layout.Controls.Add(new Label
        {
            Text = "Результат:",
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Margin = new Padding(3, 15, 3, 5)
        }, 0, 6);
        layout.SetColumnSpan(layout.GetControlFromPosition(0, 6)!, 2);

        resultTextBox = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            MinimumSize = new Size(100, 100),
            Dock = DockStyle.Fill,
            ScrollBars = ScrollBars.Vertical
        };
        layout.Controls.Add(resultTextBox, 0, 7);
        layout.SetColumnSpan(resultTextBox, 2);

        generateButton = new Button
        {
            Text = "Сгенерировать оправдание",
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6),
            Anchor = AnchorStyles.Right
        };
        generateButton.Click += (_, _) =>
            GenerateClicked?.Invoke(this, EventArgs.Empty);

        layout.Controls.Add(generateButton, 0, 8);
        layout.SetColumnSpan(generateButton, 2);

        layout.RowCount = 9;
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        Controls.Add(layout);
    }

    public void ShowData(UserData data)
    {
        nameValueLabel.Text = data.Name;
        genderValueLabel.Text = data.Gender == Gender.Female ? "Женский" : "Мужской";
    }

    public void ShowResult(string result)
    {
        resultTextBox.Text = result;
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

    private sealed record SituationItem(Situation Value, string Name)
    {
        public override string ToString() => Name;
    }
}
