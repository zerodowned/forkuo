using Server;

namespace CustomsFramework.Systems.ShardControl
{
    public abstract class BaseSettings
    {
        public abstract override string ToString();

        protected abstract void Serialize(GenericWriter writer);
        protected abstract void Deserialize(GenericReader reader);
    }
}
