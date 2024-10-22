using KBbeat;

public class HitNoteObject : NoteObject
{
    public override NoteType noteType => NoteType.HIT;

    public override bool Detach(Score score, Latency latency, ReportCallback reportCallback = null)
    {
        return true;
    }

    public override void Die()
    {
        LevelPlayer.Instance.HitNotePool.Release(this);
    }
}