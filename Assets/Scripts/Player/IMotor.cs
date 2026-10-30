using UnityEngine;

public interface IMotor
{
    bool IsGrounded { get; }

    bool Enabled { get; set; }

    void Launch(float height, float forwardSpeed);

    void Teleport(Vector3 position);
}
