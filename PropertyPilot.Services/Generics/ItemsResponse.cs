namespace PropertyPilot.Services.Generics;

public class ItemsResponse<T>
{
    public T Items { get; set; }

    public ItemsResponse(T items)
    {
        Items = items;
    }
}