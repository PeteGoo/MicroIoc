namespace MicroIoc
{
    public class InjectedConstructorParam<T> : InjectedMemberBase
    {
        public InjectedConstructorParam(string name, T value)
        {
            MemberName = name;
            MemberValue = value;
        }

        public override string DeriveFullName<TClass>()
        {
            return typeof (TClass).ConstructorParamPattern(MemberName);
        }
    }
}