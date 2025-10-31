public class CollectItemGoal : Task
{
    public string ItemName => Data.ItemName;
    public int QuantityNeeded => Data.QuantityNeeded;

    //private int   _quantityCollected = 0;

    private CollectItemGoalSO Data => this.TypedData<CollectItemGoalSO>();

    public CollectItemGoal(CollectItemGoalSO data)
        : base(data)
    {
        // nothing to do
    }
}