﻿using Sandbox;
using System;
using System.Linq;

namespace PropHunt
{
	public partial class PropHuntPlayer : Player
	{
        public bool LockRotatation { get; private set; }
        public bool HideTeamSelection { get; set; }

        [Net]
        public int TeamIndex { get; private set; }

        private float MaxHealth = 100f;

        public PropHuntPlayer()
        {
            Inventory = new BaseInventory(this);
        }

        public override void Respawn()
		{
            Inventory.DeleteContents();
            SetModel("models/citizen/citizen.vmdl");

			//
			// Use WalkController for movement (you can make your own PlayerController for 100% control)
			//
			Controller = new WalkController();

			//
			// Use StandardPlayerAnimator  (you can make your own PlayerAnimator for 100% control)
			//
			Animator = new StandardPlayerAnimator();

            if(PropHuntGame.GetTeam(TeamIndex) is SeekerTeam)
            {
                Camera = new FirstPersonCamera();
                GiveWeapons();
            }
            else
                Camera = new PropCamera();

            EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			base.Respawn();
        }

		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		public override void Simulate(Client cl)
		{
			base.Simulate(cl);

			//
			// If you have active children (like a weapon etc) you should call this to 
			// simulate those too.
			//
			SimulateActiveChild(cl, ActiveChild);

            if(PropHuntGame.GetTeam(TeamIndex) is PropTeam)
            {
                if(Input.Pressed(InputButton.Use))
                {
                    Vector3 startPos = Position;
                    startPos += Vector3.Up * (CollisionBounds.Maxs.z * Scale) * 0.75f;
                    TraceResult tr = Trace.Ray(startPos, startPos + EyeRot.Forward * 100).UseHitboxes().Ignore(this).Run();
                    if(tr.Hit && tr.Body.IsValid() && tr.Entity is Prop prop && tr.Body.BodyType == PhysicsBodyType.Dynamic)
                    {
                        UseProp(prop);
                    }
                }

                if(Input.Pressed(InputButton.Attack2))
                {
                    LockRotatation = !LockRotatation;
                }
            }

            if(Inventory.Count() > 1)
            {
                //if(Input.Pressed(InputButton.Attack2))
                if(Input.MouseWheel != 0)
                {
                    int slot = Inventory.GetActiveSlot();
                    if(Input.MouseWheel > 0)
                        slot++;
                    else
                        slot--;

                    if(slot >= Inventory.Count())
                        slot = 0;
                    else if(slot < 0)
                        slot = Inventory.Count() - 1;

                    Inventory.SetActiveSlot(slot, false);
                }
            }
        }

		public override void OnKilled()
		{
			base.OnKilled();

			EnableDrawing = false;
            Inventory.DeleteContents();
		}

        public void SetTeam(int team)
        {
            if(TeamIndex == team)
                return;

            PropHuntGame.GetTeam(TeamIndex)?.Leave(this);
            TeamIndex = team;
            PropHuntGame.GetTeam(TeamIndex)?.Join(this);
        }

        private void UseProp(Prop prop)
        {
            if(GetModel() == prop.GetModel())
                return;

            SetModel(prop.GetModel());
            Scale = prop.Scale;
            RenderColor = prop.RenderColor;

            Animator = new PropHuntAnimator();
            Controller = new PropHuntController();
            this.EnableAllCollisions = false;

            float multiplier = Math.Clamp(Health / MaxHealth, 0, 1);
            float health = (float)Math.Pow(prop.CollisionBounds.Volume, 0.5f) * 0.5f;
            health = (float)Math.Round(health / 5) * 5;
            MaxHealth = health;
            Health = health * multiplier;
        }

        private void GiveWeapons()
        {
            Inventory.Add(new Shotgun(), true);
            Inventory.Add(new SMG(), true);
        }
    }
}
