using UnityEngine;
using System.Collections.Generic;
using System;

class EntityScanner
{
    private float _viewDistance;
    public float ViewDistance
    {
        get => _viewDistance;
        set => _viewDistance = value;
    }

    private float _viewAngle;
    public float ViewAngle
    {
        get => _viewAngle;
        set => _viewAngle = value;
    }

    private Transform _sourceTransform;
    public Transform SourceTransform
    {
        get => _sourceTransform;
        set => _sourceTransform = value;
    }

    private Vector3 _eyeOffset = new Vector3(0f, 1.5f, 0f);
    public Vector3 EyeOffset
    {
        get => _eyeOffset;
        set => _eyeOffset = value;
    }

    private int _targetMask;
    public int TargetMask
    {
        get => _targetMask;
        set => _targetMask = value;
    }

    private int _obstacleMask;
    public int ObstacleMask
    {
        get => _obstacleMask;
        set => _obstacleMask = value;
    }

    private Collider[] _candidates = new Collider[16]; // Adjust size as needed, e.x. 16 friendly NPCs in one space


    public IEnumerable<Collider> doScan(int maxObjects = 0)
    {
        var eyePosition = SourceTransform.position + this.EyeOffset;

        Array.Clear(_candidates, 0, _candidates.Length); // Reset candidates array

        // OverlapSphereNonAlloc will not allocate anything to memory, and a sphere is also quicker than a box
        int candidateCount = Physics.OverlapSphereNonAlloc(SourceTransform.position, ViewDistance / 2f, _candidates, _targetMask);

        int count = 0;
        foreach (Collider target in _candidates)
        {
            if (target == null) continue;
            if (target.transform == SourceTransform) continue;

            float distanceToTarget = Vector3.Distance(eyePosition, target.transform.position);
            if (distanceToTarget > ViewDistance) continue;

            Vector3 dirToTarget = (target.transform.position - eyePosition).normalized;
            float angleToTarget = Vector3.Angle(SourceTransform.forward, dirToTarget);
            if (angleToTarget > (ViewAngle / 2f)) continue;

            // float distanceToTarget = Vector3.Distance(eyePosition, Target.transform.position);
            if (!Physics.Raycast(eyePosition, dirToTarget, distanceToTarget, _obstacleMask))
            {
                yield return target;

                count++;
                if (maxObjects > 0 && count >= maxObjects) break;
            }
        }
    }
}
