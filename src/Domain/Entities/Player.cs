namespace Domain.Entities;

public sealed class Player
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsSubstitute { get; private set; }

    private Player(){}

    private Player(string name, bool isSubstitute)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name is required.",nameof(name));

        Name = name.Trim();
        IsSubstitute = isSubstitute;
    }

    public static Player Create(string name,bool isSubstitute = false) => new(name, isSubstitute);

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Player name is required.",nameof(name));

        Name = name.Trim();
    }
}