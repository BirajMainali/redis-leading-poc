public class CacheOptions<T>
{
    public string Key { get; set; }
    public TimeSpan Duration { get; set; }
    public Func<Task<T>> Reader { get; set; }
}

public class CacheConfiguration
{
    public string Organization { get; set; }
}
