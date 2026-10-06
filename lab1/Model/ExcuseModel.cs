namespace lab1.Model;

public sealed class ExcuseModel
{
    public UserData Data { get; private set; } = new();

    public Situation Situation { get; private set; } = Situation.Study;

    public string? LastExcuse { get; private set; }

    public event EventHandler? StateChanged;

    private readonly Dictionary<Situation, Dictionary<Gender, List<string>>> _excuses = new()
    {
        { Situation.Work, new Dictionary<Gender, List<string>>(){
            {Gender.Female, [
                    "не смогла подключиться к корпоративной сети",
                    "ждала, пока техподдержка поднимет сервер",
                    "напилась",
                    "меня срочно вызвали к врачу"
                ]
            },
            {Gender.Male, [
                    "не смог подключиться к корпоративной сети",
                    "ждал, пока техподдержка поднимет сервер",
                    "напился",
                    "меня срочно вызвали к врачу"
                ]
            }
        }},
        
        { Situation.Forgetfulness, new Dictionary<Gender, List<string>>(){
            {Gender.Female, [
                    "записала задачу не туда",
                    "перепутала даты дедлайна"
                ]
            },
            {Gender.Male, [
                    "записал задачу не туда",
                    "перепутал даты дедлайна"
                ]
            }
        }},
        
        { Situation.Study, new Dictionary<Gender, List<string>>(){
            {Gender.Female, [
                    "заболела, но справку по ОРВИ не выдали",
                    "было слишком много других лабораторных",
                    "кошка рожала"
                ]
            },
            {Gender.Male, [
                    "заболел, но справку по ОРВИ не выдали",
                    "было слишком много других лабораторных",
                    "кошка рожала"
                ]
            }
        }},

        { Situation.Lateness, new Dictionary<Gender, List<string>>(){
            {Gender.Female, [
                    "помогала бабушке перейти дорогу",
                    "попала в пробку",
                    "задержали на работе"
                ]
            },
            {Gender.Male, [
                    "помогал бабушке перейти дорогу",
                    "попал в пробку",
                    "задержали на работе"
                ]
            }
        }}
    };

    public void SetData(UserData data)
    {
        Validate(data);

        Data = new UserData
        {
            Name = data.Name.Trim(),
            Gender = data.Gender
        };

        LastExcuse = null;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetSituation(Situation situation)
    {
        Situation = situation;
        LastExcuse = null;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public string GenerateExcuse()
    {
        var excuse = _excuses[Situation][Data.Gender][Random.Shared.Next(_excuses[Situation].Count)];
        
        LastExcuse = Situation switch
        {
            Situation.Study =>
                $"Я {Past("не смог", "не смогла")} нормально подготовиться к учебе, потому что {excuse}", 

            Situation.Work =>
                $"Я {Past("не успел", "не успела")} выполнить рабочую задачу, потому что {excuse}.",

            Situation.Lateness =>
                $"Я {Past("опоздал", "опоздала")}, потому что {excuse}.",

            Situation.Forgetfulness =>
                $"Я {Past("забыл", "забыла")} сделать это, потому что {excuse}.",

            _ => throw new InvalidOperationException("Неизвестная ситуация.")
        };

        StateChanged?.Invoke(this, EventArgs.Empty);
        return LastExcuse;
    }

    private string Past(string male, string female) =>
        Data.Gender == Gender.Female ? female : male;

    private static void Validate(UserData data)
    {
        if (string.IsNullOrWhiteSpace(data.Name))
            throw new ArgumentException("Поле «Имя» не может быть пустым.");

        if (data.Name.Trim().Length > 50)
            throw new ArgumentException("Имя не должно содержать более 50 символов.");
    }
}
