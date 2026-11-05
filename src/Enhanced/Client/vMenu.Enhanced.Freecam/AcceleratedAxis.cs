namespace vMenu.Enhanced.Freecam;

internal sealed class AcceleratedAxis
{
    private const float DecelerationRatio = 5f;

    private float _speed;

    internal void Reset() => _speed = 0f;

    internal float Step(float input, float maxSpeed, float secondsToFullSpeed, float delta)
    {
        var target = input * maxSpeed;

        if (secondsToFullSpeed <= 0f)
        {
            _speed = target;

            return _speed;
        }

        var acceleration = maxSpeed / secondsToFullSpeed;
        var limit = MathF.Abs(target);

        if (limit >= MathF.Abs(_speed))
        {
            _speed = Math.Clamp(_speed + (MathF.Sign(input) * acceleration * delta), -limit, limit);
        }
        else
        {
            var slowdown = MathF.Min(acceleration * DecelerationRatio * delta, MathF.Abs(_speed) - limit);

            _speed -= slowdown * MathF.Sign(_speed);
        }

        return _speed;
    }
}
