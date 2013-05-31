using System;

namespace MicroIoc
{
    public class ContainerConfiguration : IConfiguration
    {
        private readonly IMicroIocContainer _container;

        public ContainerConfiguration(IMicroIocContainer container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            _container = container;
        }

        public IConfiguration Configure<T>(InjectedMemberBase injection)
        {
            var fullName = injection.DeriveFullName<T>();
            _container.RegisterInstance(injection.MemberValue.GetType(), injection.MemberValue, fullName);
            return this;
        }
    }
}
