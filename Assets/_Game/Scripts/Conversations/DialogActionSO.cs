using UnityEngine;

public class DialogActionContext
{
    // TODO: we will need context eventually
}

public abstract class DialogActionSO : ScriptableObject
{
    public abstract void Execute(DialogActionContext context);
}
