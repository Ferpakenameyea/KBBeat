public enum Directions
{
    LEFT, RIGHT
}

public struct Score
{
    public readonly int score;
    public readonly string name;
    public readonly float accuracy;

    public readonly static Score CUTE = new(3, "CUTE", 1.0f);
    public readonly static Score TOOHEAVY = new(2, "TOOHEAVY", 0.75f);
    public readonly static Score TOOWEAK = new(2, "TOOWEAK", 0.75f);
    public readonly static Score FINE = new(2, "FINE", 0.7f);
    public readonly static Score AWFUL = new(0, "AWFUL", 0);
    public readonly static Score MISS = new(0, "MISS", 0);

    private Score(int score, string name, float accuracy)
    {
        this.score = score;
        this.name = name;
        this.accuracy = accuracy;
    }

    public static bool operator >(in Score v1, in Score v2)
    {
        return v1.score > v2.score;
    }

    public static bool operator <(in Score v1, in Score v2)
    {
        return v2.score > v1.score;
    }

    public override string ToString()
    {
        return this.name;
    }
}

public enum ForceScore
{
    ACCURATE,
    TOOHEAVY,
    TOOWEAK,
    UNJUDGED
}

public enum NoteType
{
    UNKNOWN,
    HIT, 
    HOLD
}

public enum Latency
{
    EARLY,
    OK,
    LATE,
    UNJUDGED
}