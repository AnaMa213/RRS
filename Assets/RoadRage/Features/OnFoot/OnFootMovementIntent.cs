using UnityEngine;

namespace RoadRage.Features.OnFoot
{
    /// <summary>
    /// Intention locale de deplacement lue depuis l'input. La synchro reseau de l'Epic 2 pourra
    /// remplacer la source de cette intention sans changer le motor local.
    /// </summary>
    public readonly struct OnFootMovementIntent
    {
        public static readonly OnFootMovementIntent Idle = new OnFootMovementIntent(Vector2.zero, Vector2.zero, false);

        public OnFootMovementIntent(Vector2 move, Vector2 look, bool sprintRequested)
        {
            Move = Vector2.ClampMagnitude(move, 1f);
            Look = look;
            SprintRequested = sprintRequested;
        }

        public Vector2 Move { get; }

        public Vector2 Look { get; }

        public bool SprintRequested { get; }

        public bool IsIdle
        {
            get { return Move.sqrMagnitude <= 0.0001f && Look.sqrMagnitude <= 0.0001f && !SprintRequested; }
        }
    }
}
