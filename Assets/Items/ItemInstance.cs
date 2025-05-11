using System;

[System.Serializable]
public class ItemInstance
{
    public ItemData Data;
    public float Condition;

    public ItemInstance(ItemData data)
    {
        Data = data;
        Condition = 1.0f;
    }
}
