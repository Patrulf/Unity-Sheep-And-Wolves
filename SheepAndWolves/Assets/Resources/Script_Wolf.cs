using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Script_Wolf {

	private Script_Grid _grid;
	private Script_GameManager _gameManager;
	private GameObject _wolfObject;

	private Vector3Int _occupiedLocation;
	private Vector3Int _targetLocation;
	private Vector3Int _reproductionLocation;

	private List<Script_Tile> _tilesWithinRange;
	private List<Script_Sheep> _sheepWithinRange;
	private List<Script_Tile> _surroundingTiles;

	private delegate void ActDelegate();
	ActDelegate _actDelegate;

	private float _health;
	private float _maxHealth;
	private float _actionDecay;


	private Script_Sheep _sheepBeingEaten;
	private int _sensingRange;
	private float _size;
	private float _movementDecay;
	private float _speed;
	private float _healthNeededToReproduce;

	private Material _material; 

	private enum DecidedAction
	{
		Wandering,
		Seeking,
		Eating,
		Reproducing,
		dying
	}

	private DecidedAction _decidedAction;

	public Script_Wolf(Vector3 p_position, Script_GameManager p_gameManager, Script_Grid p_grid, Quaternion p_rotation)
	{

		_wolfObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_wolfObject.name = "WolfObject";
		_wolfObject.transform.position = p_position;
		_wolfObject.transform.rotation = p_rotation;
		_material = _wolfObject.GetComponent<Renderer>().material;
		_material.color = Color.black;



		_speed = 0.033f;
		_size = 0.5f;
		_movementDecay = 0.01f;
		_actionDecay = 0.1f;
		_sensingRange = 2;
		_sheepBeingEaten = null;
		_maxHealth = 10.0f;
		_health = 5.0f;
		_healthNeededToReproduce = _maxHealth * 0.75f;


		_decidedAction = DecidedAction.Wandering;
		_actDelegate = null;
		_tilesWithinRange = new List<Script_Tile> ();
		_sheepWithinRange = new List<Script_Sheep> ();
		_surroundingTiles = new List<Script_Tile> ();
		_grid = p_grid;

		_occupiedLocation = GetCurrentGridPosition ();
		_targetLocation = GetCurrentGridPosition();
		OccupyNewLocation (GetCurrentGridPosition ());
	}

	~Script_Wolf()
	{
		_gameManager.DestroyObject (_wolfObject);
	}

	public GameObject GetWolfObject()
	{
		return _wolfObject;
	}

	public Vector3Int GetCurrentGridPosition()
	{
		int tempX = Mathf.RoundToInt (GetPosition().x);
		int tempY = Mathf.RoundToInt (GetPosition ().y);
		int tempZ = Mathf.RoundToInt (GetPosition ().z);

		return new Vector3Int (tempX, tempY, tempZ);
	}

	public Vector3 GetPosition()
	{
		return _wolfObject.transform.position;
	}

	private void OccupyNewLocation(Vector3Int p_positionToOccupy)
	{

		Script_Tile currentlyOccupiedTile = _grid.AccessGridTile (_occupiedLocation.x, _occupiedLocation.z);
		currentlyOccupiedTile.SetOccupiedByWolf (false);

		Vector3Int pos = p_positionToOccupy;
		Script_Tile tileToOccupy = _grid.AccessGridTile (pos.x, pos.z);

		tileToOccupy.SetOccupiedByWolf (true);
		_occupiedLocation = pos;

	}

	public List<Script_Tile> GetTilesWithinRange(int p_range)
	{

		List<Script_Tile> tilesWithinRange = new List<Script_Tile>();
		int xCurrent = GetCurrentGridPosition ().x;
		int zCurrent = GetCurrentGridPosition ().z;


		int xMin = xCurrent;
		int xMax = xCurrent;
		int zMin = zCurrent;
		int zMax = zCurrent;

		for (int x = xCurrent - p_range; x <= xCurrent; x++) {
			if (x < 0)
				continue;

			xMin = x;
			break;
		}
		for (int x = xCurrent + p_range; x >= xCurrent; x--) {
			if (x >= _grid.GetWidth ())
				continue;

			xMax = x;
			break;
		}
		for (int z = zCurrent - p_range; z <= zCurrent; z++) {
			if (z < 0)
				continue;

			zMin = z;
			break;
		}
		for (int z = zCurrent + p_range; z >= zCurrent; z--) {
			if (z >= _grid.GetHeight ())
				continue;

			zMax = z;
			break;
		}

		for (int z = zMin; z <= zMax; z++) {
			for (int x = xMin; x <= xMax; x++) {
				Script_Tile tile = _grid.AccessGridTile (x, z);
				tilesWithinRange.Add (tile);
			}
		}

		return tilesWithinRange;


	}

	private List<Script_Sheep> GetSheepWithinRange(int p_range)
	{

		List<Script_Sheep> allSheep = _grid.GetSheep ();
		List<Script_Sheep> SheepWithinRange = new List<Script_Sheep>();
		int xCurrent = GetCurrentGridPosition().x;
		int zCurrent = GetCurrentGridPosition().z;


		int xMin = xCurrent;
		int xMax = xCurrent;
		int zMin = zCurrent;
		int zMax = zCurrent;

		for (int x = xCurrent - p_range; x <= xCurrent; x++) {
			if (x < 0)
				continue;

			xMin = x;
			break;
		}
		for (int x = xCurrent + p_range; x >= xCurrent; x--) {
			if (x >= _grid.GetWidth ())
				continue;

			xMax = x;
			break;
		}
		for (int z = zCurrent - p_range; z <= zCurrent; z++) {
			if (z < 0)
				continue;

			zMin = z;
			break;
		}
		for (int z = zCurrent + p_range; z >= zCurrent; z--) {
			if (z >= _grid.GetHeight ())
				continue;

			zMax = z;
			break;
		}

		for (int z = zMin; z <= zMax; z++) {
			for (int x = xMin; x <= xMax; x++) {
				foreach (Script_Sheep sheep in allSheep.ToList())
				{
					if (sheep != null && sheep.GetSheepObject() != null) {
						int wolfX = sheep.GetCurrentGridPosition ().x;
						int wolfZ = sheep.GetCurrentGridPosition ().z;						
						if (wolfX >= xMin && wolfX <= xMax
						   && wolfZ >= zMin && wolfZ <= zMax) {
							SheepWithinRange.Add (sheep);
						}
					}
				}
			}
		}

		if (SheepWithinRange.Count > 0)
			return SheepWithinRange;


		return null;

	}

	public void Sense()
	{
		_tilesWithinRange = GetTilesWithinRange (_sensingRange);
		_sheepWithinRange = GetSheepWithinRange (_sensingRange);
		_surroundingTiles = GetTilesWithinRange (1);
	}

	private bool AtTargetLocation()
	{
		if (Vector3.Distance (GetPosition (), _targetLocation) <= _size) 
			return true;
		else
			return false;
	}

	bool DecideToSeekGrass()
	{
		if (_sheepWithinRange != null) {
			if (_sheepWithinRange.Count > 0) {
				return true;
			}
		}
		return false;
	}

	bool DecideToWander()
	{
		if (_tilesWithinRange.Count > 0) {
			return true;
		}
		return false;
	}



	private void DecideWolfToSeek()
	{
		List<Script_Sheep> sheepWithinRangeExcludingPosition = _sheepWithinRange;

		foreach (Script_Sheep sheep in _sheepWithinRange.ToList()) {
			if (sheep != null && sheep.GetSheepObject() != null) {
				sheepWithinRangeExcludingPosition.Add (sheep);
			}
		}

		Script_Sheep sheepToRemove = null;

		foreach(Script_Sheep sheep in _sheepWithinRange.ToList())
		{
			if (sheep != null && sheep.GetSheepObject() != null) {
				if (GetCurrentGridPosition () == sheep.GetCurrentGridPosition ())
					sheepToRemove = sheep;
			}
		}
		if (sheepToRemove != null)
			sheepWithinRangeExcludingPosition.Remove (sheepToRemove);

		List<Script_Sheep> availableSheep = new List<Script_Sheep>();
		foreach (Script_Sheep sheep in sheepWithinRangeExcludingPosition) {
			if (sheep != null && sheep.GetSheepObject() != null) {
				int sheepX = sheep.GetCurrentGridPosition ().x;
				int sheepZ = sheep.GetCurrentGridPosition ().z;
				if (_grid.AccessGridTile (sheepX, sheepZ).GetOccupiedByWolf () == false) {
					availableSheep.Add (sheep);
				}
			}
		}
		if (availableSheep.Count > 0) {

			_targetLocation = GetNearestSheep (availableSheep).GetCurrentGridPosition ();
			OccupyNewLocation (_targetLocation);
		}
		else {
			_targetLocation = GetCurrentGridPosition ();
			OccupyNewLocation (_targetLocation);
		}
	}
	private Script_Sheep GetNearestSheep(List<Script_Sheep> p_sheepList)
	{
		List<Script_Sheep> nearestSheepWithinRange = new List<Script_Sheep> ();
		if ( p_sheepList != null && p_sheepList.Count > 0) {
			int[] distanceArray = new int[p_sheepList.Count];


			for (int i = 0; i < p_sheepList.Count; i++) {
				if (p_sheepList [i] != null && p_sheepList [i].GetSheepObject () != null) {
					distanceArray [i] = (int)Vector3Int.Distance (GetCurrentGridPosition (), p_sheepList [i].GetCurrentGridPosition ());
				}
			}

			int shortestDistance = int.MaxValue;
			for (int j = 0; j < p_sheepList.Count; j++) {

				if (shortestDistance > distanceArray [j]) {
					nearestSheepWithinRange.Clear ();
					shortestDistance = distanceArray [j];
				}

				if (shortestDistance == distanceArray [j]) {
					nearestSheepWithinRange.Add (p_sheepList [j]);
				}

			}

			Script_Sheep randomCloseSheep = null;
			if (nearestSheepWithinRange != null) {
				int randomNumber = Random.Range (0, nearestSheepWithinRange.Count);
				randomCloseSheep = nearestSheepWithinRange [randomNumber];
			}

			if (randomCloseSheep != null)
				return randomCloseSheep;
		}
		return null;
	}
	private void DecideWanderingLocation()
	{

		List<Script_Tile> tilesWithinRangeExcludingPosition = _tilesWithinRange;
		Script_Tile tileToRemove = null;

		foreach(Script_Tile tile in _tilesWithinRange.ToList())
		{
			if (GetCurrentGridPosition () == tile.GetPositionAsInt ())
				tileToRemove = tile;
		}
		if (tileToRemove != null)
			tilesWithinRangeExcludingPosition.Remove (tileToRemove);

		List<Script_Tile> availableTiles = new List<Script_Tile>();
		foreach (Script_Tile tile in tilesWithinRangeExcludingPosition) {
			if (tile.GetOccupiedByWolf () == false) {
				availableTiles.Add (tile);
			}
		}
		int randomNumber = Random.Range(0,availableTiles.Count-1);
		if (availableTiles.Count > 0) {
			_targetLocation = availableTiles [randomNumber].GetPositionAsInt ();
			OccupyNewLocation (_targetLocation);
		}
		else {
			_targetLocation = GetCurrentGridPosition ();
			OccupyNewLocation (_targetLocation);
		}
	}

	private bool DecideToEatSheep()
	{
		int x = GetCurrentGridPosition().x;
		int z = GetCurrentGridPosition().z;

		if (_sheepWithinRange != null) {
			foreach (Script_Sheep sheep in _sheepWithinRange.ToList()) {
				if (sheep != null && sheep.GetSheepObject() != null) {
					int sheepX = sheep.GetCurrentGridPosition ().x;
					int sheepZ = sheep.GetCurrentGridPosition ().z;
					if (GetCurrentGridPosition () == sheep.GetCurrentGridPosition () && (_grid.AccessGridTile (sheepX, sheepZ).GetOccupiedByWolf () != true
					   || _occupiedLocation.x == sheepX && _occupiedLocation.z == sheepZ)) {

						Vector3Int occupyLocation = new Vector3Int (x, 0, z);
						OccupyNewLocation (occupyLocation);
						_sheepBeingEaten = sheep;
						return true;
					}
				}
			}
		}

		return false;

	}

	private bool DecideToReproduce()
	{
		if (_surroundingTiles != null && _health >= _healthNeededToReproduce) {

			List<Script_Tile> surroundingTilesExcludingPosition = _surroundingTiles;
			Script_Tile tileToRemove = null;

			foreach (Script_Tile tile in _surroundingTiles) {
				if (GetCurrentGridPosition () == tile.GetPositionAsInt ())
					tileToRemove = tile;
			}
			if (tileToRemove != null)
				surroundingTilesExcludingPosition.Remove (tileToRemove);

			List<Script_Tile> availableTiles = new List<Script_Tile> ();
			foreach (Script_Tile tile in surroundingTilesExcludingPosition) {
				if (tile.GetOccupiedByWolf () == false) {
					availableTiles.Add (tile);
				}
			}
			if (availableTiles.Count > 0) {
				return true;
			}

		}
		return false;
	}

	private Vector3Int DecideReproductionTile()
	{
		List<Script_Tile> tilesWithinRangeExcludingPosition = _surroundingTiles;
		Script_Tile tileToRemove = null;

		foreach(Script_Tile tile in _surroundingTiles)
		{
			if (GetCurrentGridPosition () == tile.GetPositionAsInt ())
				tileToRemove = tile;
		}
		if (tileToRemove != null)
			tilesWithinRangeExcludingPosition.Remove (tileToRemove);

		List<Script_Tile> availableTiles = new List<Script_Tile>();
		foreach (Script_Tile tile in tilesWithinRangeExcludingPosition) {
			if (tile.GetOccupiedByWolf () == false) {
				availableTiles.Add (tile);
			}
		}
		int randomNumber = Random.Range(0,availableTiles.Count-1);
		if (availableTiles.Count > 0) {
			return availableTiles [randomNumber].GetPositionAsInt ();
		}
		return Vector3Int.zero;
	}

	private bool DecideToDie()
	{
		if (_health <= 0.0f) {
			return true;
		}

		return false;
	}

	public void Decide()
	{
		_sheepBeingEaten = null;

		if (DecideToDie ()) {
			_decidedAction = DecidedAction.dying;
			_actDelegate = new ActDelegate (Die);
		}
	
		if (DecideToReproduce () && !DecideToDie()) {
			_reproductionLocation =  DecideReproductionTile ();
			Script_Tile currentlyOccupiedTile = _grid.AccessGridTile (_reproductionLocation.x, _reproductionLocation.z);
			currentlyOccupiedTile.SetOccupiedByWolf (false);
			_decidedAction = DecidedAction.Reproducing;
			_actDelegate = new ActDelegate (Reproduce);
		}

		if (DecideToEatSheep() && !DecideToDie() && !DecideToReproduce()) {
			OccupyNewLocation(GetCurrentGridPosition ());
			_decidedAction = DecidedAction.Eating;
			_actDelegate = new ActDelegate (EatSheep);
		}

		if (DecideToSeekGrass () && !DecideToEatSheep() && !DecideToReproduce() && !DecideToDie()) {
			if (AtTargetLocation () || _decidedAction != DecidedAction.Seeking) {
				DecideWolfToSeek ();
			}
			_decidedAction = DecidedAction.Seeking;
			_actDelegate = new ActDelegate (MoveToLocation);
		}


		if (DecideToWander () && !DecideToSeekGrass() && !DecideToEatSheep() && !DecideToReproduce() && !DecideToDie()) {
			if (AtTargetLocation() || _decidedAction != DecidedAction.Wandering)
				DecideWanderingLocation ();
			
			_decidedAction = DecidedAction.Wandering;
			_actDelegate = new ActDelegate (MoveToLocation);
		}
	}

	private void MoveToLocation()
	{
		
		_health -= _movementDecay;

		if (Vector3.Distance (_wolfObject.transform.position, _targetLocation) > _size) {
			_wolfObject.transform.position += (_targetLocation - _wolfObject.transform.position).normalized * _speed;
		} else
			_wolfObject.transform.position = _targetLocation;
	}

	public void Act()
	{
		if (_actDelegate != null)
			_actDelegate ();
	}

	private void EatSheep()
	{
		float feedingHealthIncrease = 0.07f;
		float damage = 0.2f;


		if (_sheepBeingEaten != null && _sheepBeingEaten.GetSheepObject() != null) {
			_wolfObject.transform.position += (_sheepBeingEaten.GetPosition() - _wolfObject.transform.position).normalized * _speed;
			_health += feedingHealthIncrease;
			_health = Mathf.Clamp (_health, 0, _maxHealth);
			if (_sheepBeingEaten != null)
				_sheepBeingEaten.DecreaseHealth (damage);
		} else {
			_health -= _actionDecay;
		}
	}

	private void Reproduce()
	{			
		float reproductionDamage = _health * 0.5f;

		if (_health >= _healthNeededToReproduce) {
			_health = reproductionDamage;
			_grid.InstantiateWolf(_reproductionLocation);

		} else {
			_health -= _actionDecay;
		}

	}

	private void Die()
	{
		Vector3Int position = GetCurrentGridPosition ();
		_grid.AccessGridTile (position.x, position.z).NourishGrass();

		Script_Tile currentlyOccupiedTile = _grid.AccessGridTile (_occupiedLocation.x, _occupiedLocation.z);
		currentlyOccupiedTile.SetOccupiedByWolf (false);

		_grid.DestroyWolf (this, _material);
		_wolfObject = null;
	}

}