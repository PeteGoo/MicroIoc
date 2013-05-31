namespace MicroIoc
{
    public interface IConfiguration
    {
        IConfiguration Configure<T>(InjectedMemberBase injection);
    }
}
