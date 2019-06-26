using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Script_GameManager : MonoBehaviour
{
	private Script_Grid Grid;
	private GameObject GridTile;

	private Vector3 cameraForward;
	private Vector3 cameraUp;
	private Vector3 cameraOffset;

	private float tileSize = 1.0f; //We know the tiles are 1.0f from looking at the cube mesh's dimensions from PrimitiveType.Cube in the inspector, I Don't really know how to define this clearly since unity already has this defined for me.
	//It is needed however to properly center the camera.

	private float width = 10.0f;
	private float height = 10.0f;

	private float _senseTimerFrequency;
	private float _decideTimerFrequency;
	private float _actTimerFrequency;

	private float cameraXPosition;
	private float cameraZPosition;
	private float cameraYPosition;

	float senseTimer;
	float decideTimer;
	float actTimer;

	private float _tileHeight;



	void Awake()
	{
		_tileHeight = -1.0f;
		_senseTimerFrequency = 1.0f;
		_decideTimerFrequency = 1.0f / 2.0f;
		_actTimerFrequency = 1.0f / 30.0f;


		senseTimer = 0.0f;
		decideTimer = 0.0f;
		actTimer = 0.0f;

		cameraXPosition =  width * 0.5f - tileSize * 0.5f;
		cameraZPosition = height * 0.5f - tileSize * 0.5f;
		cameraYPosition = 10.0f;

		cameraForward = new Vector3 (0, -1, 0);
		cameraUp = new Vector3 (0, 0, 1);

		Camera.main.transform.position = new Vector3 (cameraXPosition, cameraYPosition, cameraZPosition);
		Camera.main.transform.rotation = Quaternion.LookRotation (cameraForward, cameraUp);

		GridTile = Resources.Load ("Prefab_GridTile") as GameObject;
		Grid = new Script_Grid(this);


		Grid.InstantiateGrid (GridTile,10,10,_tileHeight,transform.rotation);
		GameObject sheep = Resources.Load ("Prefab_Sheep") as GameObject;
		Grid.CreateSheep (sheep,10,10,0,transform.rotation,10);

		GameObject wolf = Resources.Load ("Prefab_Wolf") as GameObject;
		Grid.CreateWolves (wolf, 10, 10, 0, transform.rotation,3);

	}

	void Update()
	{


		if (Timer (ref senseTimer, _senseTimerFrequency)) {
			Grid.Sense();
		}

		if (Timer (ref decideTimer, _decideTimerFrequency)) {
			Grid.Decide ();
		}

		if (Timer (ref actTimer, _actTimerFrequency)) {
			Grid.Act ();
		}
	}

	public static bool Timer(ref float p_currentTimer, float p_timerResetValue)
	{
		if (p_currentTimer < p_timerResetValue) {
			p_currentTimer += Time.deltaTime;
			return false;
		}
		p_currentTimer = 0.0f;
		return true;
	}

	public GameObject InstantiateObject(GameObject p_object, Vector3 p_position, Quaternion p_rotation)
	{
		return Instantiate (p_object, p_position, p_rotation);
	}

	public GameObject InstantiateObject(GameObject p_object, Vector3 p_position)
	{
		return Instantiate (p_object, p_position, transform.rotation);
	}



	public GameObject InstantiateSheep (Vector3 p_position)
	{
		GameObject sheep = Resources.Load ("Prefab_Sheep") as GameObject;
		return Instantiate (sheep, p_position, transform.rotation);
	}


	public void DestroyObject(GameObject p_objectToDestroy)
	{
		Destroy (p_objectToDestroy);
	}

	public void DestroyMaterial(Material p_objectToDestroy)
	{
		Destroy (p_objectToDestroy);
	}
		



}