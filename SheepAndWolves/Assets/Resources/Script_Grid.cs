using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Config;

public class Script_Grid {

	private float _matureTimerCurrent;
	private float _matureTime;
	private Script_GameManager _gameManager;
	private List<Script_Sheep> _sheepList;
	private List<Script_Wolf> _wolfList;


	public Script_Grid(Script_GameManager p_creator)
	{
		_wolfList = new List<Script_Wolf> ();
		_sheepList = new List<Script_Sheep> ();
		_gameManager = p_creator;
	}

	Script_Tile[] _grid = new Script_Tile[0];


	private int _width = 0;
	private int _height = 0;

	public void InstantiateGrid (GameObject p_tileObject, int p_width, int p_height, float p_yOffset, Quaternion p_rotation )
	{
		_width = p_width;
		_height = p_height;

		System.Array.Resize (ref _grid, p_width * p_height);


		for (int z = 0; z < _width; z++) {
			for (int x = 0; x < _height; x++) {
				if (Random.Range (0.0f, 10.0f) > 6.0f) {
					Script_Tile myTile = new Script_Tile (this, GrassStates.Grass,x,(int)p_yOffset,z,p_rotation);
					SetGridTile (x, z, myTile);
				} else {
					Script_Tile myTile = new Script_Tile (this, GrassStates.Dirt,x,(int)p_yOffset,z,p_rotation);
					SetGridTile (x, z, myTile);
				}
					

			}
		}			
	}

	public void CreateSheep(GameObject p_tileObject, int p_width, int p_height, float p_yOffset, Quaternion p_rotation, int p_numberOfSheep)
	{
		int currentAmountOfSheep = 0;
		while (currentAmountOfSheep < p_numberOfSheep) {
			int x = Random.Range (0, p_width);
			int z = Random.Range (0, p_height);

			while (AccessGridTile (x, z).GetOccupiedBySheep () == true) {
				x = Random.Range (0, p_width);
				z = Random.Range (0, p_height);
			}

			Vector3 entityPosition = new Vector3 (x, p_yOffset, z);
			Script_Sheep mySheep = new Script_Sheep (entityPosition, _gameManager, this, p_rotation);
			_sheepList.Add (mySheep);
			currentAmountOfSheep++;
		}
	}

	public void CreateWolves(GameObject p_tileObject, int p_width, int p_height, float p_yOffset, Quaternion p_rotation, int p_numberOfWolves)
	{
		int currentAmountOfWolves = 0;
		while (currentAmountOfWolves < p_numberOfWolves) {
			int x = Random.Range (0, p_width);
			int z = Random.Range (0, p_height);

			while (AccessGridTile (x, z).GetOccupiedBySheep () == true) {
				x = Random.Range (0, p_width);
				z = Random.Range (0, p_height);
			}

			Vector3 entityPosition = new Vector3 (x, p_yOffset, z);
			Script_Wolf mySheep = new Script_Wolf (entityPosition, _gameManager, this, p_rotation);
			_wolfList.Add (mySheep);
			currentAmountOfWolves++;
		}
	}

	public void Sense()
	{
		foreach (Script_Tile tile in _grid) {
			tile.Sense ();
		}

		foreach (Script_Sheep sheep in _sheepList.ToList())
		{
			sheep.Sense ();
		}

		foreach (Script_Wolf wolf in _wolfList.ToList()) {
			wolf.Sense ();
		}
	}

	public void Decide()
	{
		foreach (Script_Tile tile in _grid) {
			tile.Decide ();
		}

		foreach (Script_Sheep sheep in _sheepList.ToList())
		{
			sheep.Decide ();
		}

		foreach (Script_Wolf wolf in _wolfList.ToList()) {
			wolf.Decide ();
		}
	}

	public void Act()
	{
		foreach (Script_Tile tile in _grid) {
			tile.Act ();
		}


		foreach (Script_Sheep sheep in _sheepList.ToList())
		{
			sheep.Act ();
		}

		foreach (Script_Wolf wolf in _wolfList.ToList()) {
			wolf.Act ();
		}
	}

	public int GetWidth()
	{
		return _width;
	}

	public int GetHeight()
	{
		return _height;
	}

	public void DestroySheep(Script_Sheep p_sheep, Material p_material)
	{
		GameObject sheepObject = p_sheep.GetSheepObject ();
		_sheepList.Remove (p_sheep);
		_gameManager.DestroyObject (sheepObject);
		_gameManager.DestroyMaterial (p_material);
	}

	public void DestroyWolf(Script_Wolf p_wolf, Material p_material)
	{
		GameObject wolfObject = p_wolf.GetWolfObject ();

		_wolfList.Remove (p_wolf);
		_gameManager.DestroyObject (wolfObject);
		_gameManager.DestroyMaterial (p_material);
	}

	public void InstantiateSheep(Vector3 p_position)
	{
		Quaternion rotation = new Quaternion (0, 0, 0, 0);
		Script_Sheep mySheep = new Script_Sheep (p_position, _gameManager, this, rotation);
		_sheepList.Add (mySheep);

	}

	public void InstantiateWolf(Vector3 p_position)
	{
		Quaternion rotation = new Quaternion (0, 0, 0, 0);
		Script_Wolf myWolf = new Script_Wolf (p_position, _gameManager, this, rotation);
		_wolfList.Add (myWolf);

	}


	public List<Script_Sheep> GetSheep()
	{
		return _sheepList;
	}

	public List<Script_Wolf> GetWolves()
	{
		return _wolfList;
	}

	public Script_Tile AccessGridTile(int p_x, int p_z)
	{
		return _grid[p_z * _width + p_x];
	}

	private void SetGridTile(int p_x, int p_z, Script_Tile p_tile)
	{
		_grid [p_z * _width + p_x] = p_tile;
	}
}
