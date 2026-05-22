using System;
using System.Reflection;
using UnityEngine;

public abstract class GameEntity : MonoBehaviour
{
    [SerializeField] public float Health = 100f;
    [SerializeField] public float MaxHealth = 100f;

    private Transform _entityTransform = null!;
    private Vector3 _lastPosition = Vector3.zero;
    public Vector3 Velocity { get; private set; }

    protected virtual void Awake()
    {
        _entityTransform = this.transform;
    }

    protected virtual void Update()
    {
        Velocity = (_entityTransform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = _entityTransform.position;
    }

    public abstract float TakeDamage(float amount);
    public abstract float Heal(float amount);
    public virtual bool IsAlive { get => Health > 0; }

    public event Action<float> OnHealthChanged;
    protected void RaiseOnHealthChanged(float health)
    {
        OnHealthChanged?.Invoke(health);
    }

    protected virtual void DoInteraction(string actionName)
    {
        Type type = this.GetType();
        while (type != null && type != typeof(GameEntity))
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                ItemAction attr = method.GetCustomAttribute<ItemAction>();
                if (attr != null && attr.ActionName == actionName)
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        method.Invoke(this, null);
                        InteractionManager.Instance.ActionFlags = attr.ActionFlags;
                        return;
                    }

                    Debug.LogWarning($"Action '{actionName}' on {this.GetType().Name} has an unsupported signature.");
                    return;
                }
            }

            type = type.BaseType;
        }

        Debug.LogWarning($"No action found for '{actionName}' in {this.GetType().Name}");
    }
}