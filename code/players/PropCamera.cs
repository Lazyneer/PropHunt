﻿using Sandbox;
using System;

namespace PropHunt
{
    public class PropCamera : ThirdPersonCamera
    {
        private float orbitDistance = 150;

        public override void Update()
        {
            AnimEntity pawn = Local.Pawn as AnimEntity;

            if(pawn == null)
                return;

            Position = pawn.Position;
            Vector3 targetPos;

            Position += Vector3.Up * Math.Max(pawn.CollisionBounds.Maxs.z * pawn.Scale * 0.75f, 8f);
            Rotation = Rotation.FromAxis(Vector3.Up, 4) * Input.Rotation;

            targetPos = Pos + Rot.Backward * orbitDistance;

            Position += Vector3.Up * 0f;
            TraceResult tr = Trace.Ray(Pos, targetPos).Ignore(pawn).Radius(8).Run();
            Position = tr.EndPos;

            FieldOfView = 70;
            Viewer = null;
        }
    }
}
