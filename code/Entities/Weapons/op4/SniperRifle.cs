using Sandbox;
using Editor;

[Library( "weapon_sniperrifle" ), HammerEntity]
[EditorModel( "models/op4/weapons/world/w_m40a1.vmdl" )]
[Title( "Sniper Rifle" ), Category( "Weapons" ), MenuCategory( "Opposing Force" )]
partial class SniperRifle : Weapon
{
	public override string ViewModelPath => "models/op4/weapons/view/v_m40a1.vmdl";
	public override string WorldModelPath => "models/op4/weapons/world/w_m40a1.vmdl";
	public override float PrimaryRate => 0.75f;
	public override float SecondaryRate => 1.0f;
	public override float ReloadTime => 2.5f;
	public override AmmoType AmmoType => AmmoType.Sniper;
	public override int ClipSize => 5;
	public override int Bucket => 5;
	public override int BucketWeight => 3;
	public override string InventoryIcon => "/ui/op4/weapons/weapon_sniperrifle.png";
	public override string InventoryIconSelected => "/ui/op4/weapons/weapon_sniperrifle_selected.png";

	[Net, Predicted]
	public bool Zoomed { get; set; } = false;

	private float? LastFov;
	//private float? LastViewmodelFov;

	public override void Spawn()
	{
		base.Spawn();

		AmmoClip = 5;
		Model = Model.Load( "models/op4/weapons/world/w_m40a1.vmdl" );
	}

	public override void AttackPrimary()
	{
		if ( Owner is not HLPlayer player ) return;
		if ( !TakeAmmo( 1 ) )
		{
			DryFire();

			if ( AvailableAmmo() > 0 )
			{
				Reload();
			}
			return;
		}

		ShootEffects();
		PlaySound( "sniper_fire" );


		ShootBullet( 0.0f, 1f, 100.0f, 2.0f );


		player.punchanglecl.x = -2;

	}

	public override void Simulate( IClient cl )
	{
		base.Simulate( cl );

		if ( Input.Pressed( InputButton.SecondaryAttack ) )
		{
			Zoomed = !Zoomed;
		}
		//Zoomed = Input.Down( InputButton.SecondaryAttack );
	}

	public void PostCameraSetup()
	{
		//base.PostCameraSetup( ref camSetup );

		float targetFov = Camera.FieldOfView;
		//float targetViewmodelFov = Camera.ViewModel.FieldOfView;
		LastFov = LastFov ?? Camera.FieldOfView;
		//LastViewmodelFov = LastViewmodelFov ?? Camera.ViewModel.FieldOfView;

		if ( Zoomed )
		{
			targetFov = 18.0f;
			//targetViewmodelFov = 0.0f;
		}

		float lerpedFov = LastFov.Value.LerpTo( targetFov, Time.Delta * 24.0f );
		//float lerpedViewmodelFov = LastViewmodelFov.Value.LerpTo( targetViewmodelFov, Time.Delta * 24.0f );

		Camera.FieldOfView = targetFov;
		//Camera.ViewModel.FieldOfView = targetViewmodelFov;

		LastFov = lerpedFov;
		//LastViewmodelFov = lerpedViewmodelFov;
	}

	public override void BuildInput()
	{
		if ( Zoomed )
		{
			(Owner as HLPlayer).ViewAngles = Angles.Lerp( (Owner as HLPlayer).OriginalViewAngles, (Owner as HLPlayer).ViewAngles, 0.2f );
		}
	}

	[ClientRpc]
	protected override void ShootEffectsRPC()
	{
		Game.AssertClient();

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}
	public override void SimulateAnimator( CitizenAnimationHelper anim )
	{
		SetHoldType(HLCombat.HoldTypes.Crossbow, anim);
		//anim.SetAnimParameter( "aim_body_weight", 1.0f );
	}
}
