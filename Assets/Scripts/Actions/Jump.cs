using UnityEngine;

/// <summary>
/// This class has a built in gravity simulator. So when player isn't in any pltaform with tag Floor the gravity pulls down.
/// It works with rigidbodies, so is possible some sort of bugs appear.
/// </summary>
public class Jump : MonoBehaviour {
	
	private bool isJumping = false;
	private bool foreverJump = false;
	private float foreverJumpVel = 0f;
	private bool gainApplied = false; // whether a jump gain was or wasn't applied in current jump loop
	
	private Crouch crouch;
	private Idle idle; // when stop jumping we need to set the player to idle behavior
	private AnimateTiledConfig jumpAC;
	private ChipmunkBody body;
	
	// Use this for initialization
	void Awake () {
		idle = GetComponent<Idle>();
		crouch = GetComponent<Crouch>();
		jumpAC = GetComponentInChildren<JumpAnimConfig>();
		body = GetComponent<ChipmunkBody>();
		reset();
	}
	
	void Update(){
		////////////////////////////////////
		//IMPORTANT: this fixes the crash when assigning the modified velocity to the body
		if (isJumping)
			return;
		Vector2 v = body.velocity;
		body.velocity = v;
		////////////////////////////////////
	}
	
	public void jump (float pJumpVel) {
		
		if (isJumping)
			return;
		
		// set the correct sprite animation
		if (crouch != null && !crouch.isCrouching()) {
			jumpAC.animComp.setFPS(jumpAC.animFPS);
			jumpAC.animComp.setRowLimits(jumpAC.rowStartAnim, jumpAC.rowLengthAnim);
			jumpAC.animComp.setColLimits(jumpAC.maxColsAnimInRow, jumpAC.colStartAnim, jumpAC.colLengthAnim);
			jumpAC.animComp.setPingPongAnim(jumpAC.pingPongAnim);
			jumpAC.animComp.Play();
		}
		
		isJumping = true;
		Vector2 v = body.velocity;
		v.y += pJumpVel;
		body.velocity = v;
	}
	
	/// <summary>
	/// Applies a gain factor to current jump velocity. Only once per jump.
	/// </summary>
	/// <param name='factor'>
	/// Factor.
	/// </param>
	public void applyGain (float factor) {
		if (!gainApplied) {
			Vector2 v = body.velocity;
			v.y *= factor;
			body.velocity = v;
			gainApplied = true;
		}
	}
	
	public void reset () {
		isJumping = true; // initialized as true in case the player is spawned in the air
		gainApplied = true;
	}
	
	public bool IsJumping () {
		return isJumping;
	}
	
	public void setForeverJump (bool val) {
		foreverJump = val;
	}
	
	public void setForeverJumpSpeed (float pJumpSpeed) {
		foreverJumpVel = pJumpSpeed;
	}
	
	public static bool beginCollisionWithScenery (ChipmunkArbiter arbiter) {
		ChipmunkShape shape1, shape2;
	    // The order of the arguments matches the order in the function name.
	    arbiter.GetShapes(out shape1, out shape2);
		
		Jump jump = shape1.GetComponent<Jump>();
		if (jump != null && GameObjectTools.isGrounded(arbiter)) {
			if (jump.foreverJump) {
				jump.jump(jump.foreverJumpVel);
			}
			else {
				jump.isJumping = false;
				jump.gainApplied = false;
				// if it was jumping then set player behavior to idle
				if (jump.idle != null && jump.crouch != null && !jump.crouch.isCrouching())
					jump.idle.setIdle(false);
			}
		}
		
		// Returning false from a begin callback means to ignore the collision
	    // response for these two colliding shapes until they separate.
		return true;
	}

	public static void endCollisionWithScenery (ChipmunkArbiter arbiter) {
		ChipmunkShape shape1, shape2;
	    // The order of the arguments matches the order in the function name.
	    arbiter.GetShapes(out shape1, out shape2);
	}
}
