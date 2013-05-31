namespace MicroIoc
{
    public abstract class InjectedMemberBase
    {
        public string MemberName { get; set; }
        public object MemberValue { get; set; }

        public abstract string DeriveFullName<T>();
    }
}