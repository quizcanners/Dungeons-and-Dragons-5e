namespace Dungeons_and_Dragons
{
   public enum Condition 
    {
        Blinded,
        Charmed, 
        Deafened,
        Frightened,
        Grappled,
        Incapacitated,
        Invisible, 
        Paralyzed,
        Petrified,
        Poisoned,
        Prone, 
        Restrained,
        Stunned,
        Unconscious
    }

    [System.Serializable] public class ConditionsSet : QuizCanners.Utils.SerializableHashSetForEnum<Condition> { }

}
