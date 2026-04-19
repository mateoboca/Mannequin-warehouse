using UnityEngine;

public static class SteeringBehaviours
{
    public static Vector3 Seek(Transform self, Vector3 target)
    {
        Vector3 dir = target - self.position;
        dir.y = 0;
        return dir.normalized;
    }

    public static Vector3 Flee(Transform self, Vector3 target)
    {
        Vector3 dir = self.position - target;
        dir.y = 0;
        return dir.normalized;
    }

    public static Vector3 Arrive(Transform self, Vector3 target, float slowRadius)
    {
        Vector3 dir = target - self.position;
        float distance = dir.magnitude;

        if (distance < 0.01f)
            return Vector3.zero;

        float speedFactor = Mathf.Clamp01(distance / slowRadius);
        return dir.normalized * speedFactor;
    }

    public static Vector3 Pursue(Transform self, Transform target, Rigidbody targetRb, float maxPredictionTime)
    {
        Vector3 targetVelocity = Vector3.zero;
        if (targetRb != null)
        {
            targetVelocity = targetRb.linearVelocity;
        }

        Vector3 toTarget = target.position - self.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        float predictionTime = Mathf.Clamp(distance / 5f, 0f, maxPredictionTime);

        Vector3 futurePos = target.position + targetVelocity * predictionTime;
        return Seek(self, futurePos);
    }

    public static Vector3 Evade(Transform self, Transform target, Rigidbody targetRb, float maxPredictionTime)
    {
        Vector3 targetVelocity = Vector3.zero;

        if (targetRb != null)
        {
            targetVelocity = targetRb.linearVelocity;
        }

        Vector3 toTarget = target.position - self.position;
        toTarget.y = 0f;

        float distance = toTarget.magnitude;
        float predictionTime = Mathf.Clamp(distance / 5f, 0f, maxPredictionTime);

        Vector3 futurePos = target.position + targetVelocity * predictionTime;
        return Flee(self, futurePos);
    }

    public static Vector3 Wander(Vector3 currentDirection, float maxAngleChange)
    {
        float randomAngle = UnityEngine.Random.Range(-maxAngleChange, maxAngleChange);
        Quaternion rotation = Quaternion.Euler(0f, randomAngle, 0f);

        Vector3 newDirection = rotation * currentDirection;
        newDirection.y = 0f;

        return newDirection.normalized;
    }
}