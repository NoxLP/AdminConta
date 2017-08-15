namespace AdConta.Models
{
    public interface IObjWithDLO<T> where T : IDataListObject
    {
        T GetDLO();
    }
}
