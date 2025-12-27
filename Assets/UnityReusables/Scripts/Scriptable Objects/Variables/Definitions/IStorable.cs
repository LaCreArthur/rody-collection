namespace UnityReusables.ScriptableObjects.Variables
{
    public interface IStorable<T>
    {
        void Save();
        T Load();
    }
}