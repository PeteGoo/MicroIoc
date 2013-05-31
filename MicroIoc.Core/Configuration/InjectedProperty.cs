namespace MicroIoc
{
    public class InjectedProperty<T> : InjectedMemberBase
    {
        public InjectedProperty(string name, T value)
        {
            MemberName = name;
            MemberValue = value;
        }

        public override string DeriveFullName<TClass>()
        {
            return typeof(TClass).PropertyPattern(MemberName);
        }
    }
}
