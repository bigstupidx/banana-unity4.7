using UnityEngine;

public class Global {

	public static int UI_LAYER_MASK = 1 << LayerMask.NameToLayer( "UI" );

	public const float GESTURE_TIME = 0.25f;
	public const float GESTURE_DISTANCE_THRESHOLD = 4;

	public const float PLAYER_WALK_SPEED = 4;

	public const float GROUND_Y = 0.0f;
	public const float WALL_TOP_Y = 14.0f;
	public const float HEAVEN_Y = 25.0f;
	public const float HELL_Y = -5.0f;

	public const float WALL_LEFT_X = -7.0f;
	public const float WALL_RIGHT_X = 7.0f;

	public static float WALL_MIN_X = WALL_LEFT_X;
	public static float WALL_MAX_X = WALL_RIGHT_X;

	public const float PROJECTILE_Z = -400;

	public const float DYING_FALL_SPEED = 20;
	public const float DYING_Z = -20;
	public const float PATROL_Z = -350;
	public const float CLIMB_Z = -300;
	public const float ON_WALL_Z = 0;

	public static void Init()
	{
		WALL_MIN_X = -(Screen.width * Camera.main.orthographicSize) / Screen.height;
		WALL_MAX_X = -WALL_MIN_X;
	}
}
