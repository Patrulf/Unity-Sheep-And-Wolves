using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Config;

public class Script_Sheep {
	
	private Script_Grid _grid;
	private Script_GameManager _gameManager;
	private GameObject _sheepObject;

	private Vector3Int _occupiedLocation;
	private Vector3Int _targetLocation;
	private Vector3Int _reproductionLocation;

	private List<Script_Tile> _tilesWithinRange;
	private List<Script_Tile> _grassWithinRange;
	private List<Script_Tile> _surroundingTiles;

	private List<Script_Wolf> _wolvesWithinRange;

	private float _size;

	private delegate void ActDelegate();
	ActDelegate _actDelegate;

	private float _health;
	private float _maxHealth;

	private float _movementDecay;
	private float _speed;
	private float _actionDecay;
	private float _evasionSpeed;

	private Script_Tile _grassBeingEaten;

	private Material _material; 

	private enum DecidedAction
	{
		Wandering,
		Seeking,
		Eating,
		Reproducing,
		Evading,
		dying
	}

	private DecidedAction _decidedAction;

	public Script_Sheep(Vector3 p_position, Script_GameManager p_gameManager, Script_Grid p_grid, Quaternion p_rotation)
	{
		
		_sheepObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		_sheepObject.name = "SheepObject";
		_sheepObject.transform.position = p_position;
		_sheepObject.transform.rotation = p_rotation;
		_material = _sheepObject.GetComponent<Renderer>().material;
		_material.color = Color.white;

		_evasionSpeed = 0.066f;
		_speed = 0.033f;
		_movementDecay = 0.01f;
		_actionDecay = 0.1f;
		_size = 0.5f;
		_grassBeingEaten = null;
		_maxHealth = 10.0f;
		_health = 5.0f;

		_decidedAction = DecidedAction.Wandering;
		_actDelegate = null;
		_tilesWithinRange = new List<Script_Tile> ();
		_grassWithinRange = new List<Script_Tile> ();
		_surroundingTiles = new List<Script_Tile> ();
		_wolvesWithinRange = new List<Script_Wolf> ();
		_grid = p_grid;


		_occupiedLocation = GetCurrentGridPosition ();
		_targetLocation = GetCurrentGridPosition();
		OccupyNewLocation (GetCurrentGridPosition ());
	}

	~Script_Sheep()
	{
		_gameManager.DestroyObject (_sheepObject);

	}

	public GameObject GetSheepObject()
	{
		return _sheepObject;
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
		return _sheepObject.transform.position;
	}
		
	private void OccupyNewLocation(Vector3Int p_positionToOccupy)
	{
		
		Script_Tile currentlyOccupiedTile = _grid.AccessGridTile (_occupiedLocation.x, _occupiedLocation.z);
		currentlyOccupiedTile.SetOccupiedBySheep (false);

		Vector3Int pos = p_positionToOccupy;
		Script_Tile tileToOccupy = _grid.AccessGridTile (pos.x, pos.z);

		tileToOccupy.SetOccupiedBySheep (true);
		_occupiedLocation = pos;

	}

	public List<Script_Wolf> GetWolvesWithinRange(int p_range)
	{
		List<Script_Wolf> wolves = _grid.GetWolves();
		List<Script_Wolf> wolvesWithinRange = new List<Script_Wolf>();
		int xCurrent = GetCurrentGridPosition ().x;
		int zCurrent = GetCurrentGridPosition ().z;

		foreach (Script_Wolf wolf in wolves.ToList() ) {
			if (wolf != null && wolf.GetWolfObject () != null) {
				if (Vector3Int.Distance (GetCurrentGridPosition (), wolf.GetCurrentGridPosition ()) <= p_range) {
					wolvesWithinRange.Add (wolf);
				}
			}
		}
		return wolvesWithinRange;

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

	private List<Script_Tile> GetGrassWithinRange(int p_range)
	{

		List<Script_Tile> grassWithinRange = new List<Script_Tile>();
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
				if (_grid.AccessGridTile(x,z).GetState() == GrassStates.Grass) {
				Script_Tile tile = _grid.AccessGridTile (x, z);
				grassWithinRange.Add (tile);
				}
			}
		}

		return grassWithinRange;

	}

	public void Sense()
	{
		_tilesWithinRange = GetTilesWithinRange (2);
		_grassWithinRange = GetGrassWithinRange (2);
		_surroundingTiles = GetTilesWithinRange (1);
		_wolvesWithinRange = GetWolvesWithinRange (3);

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
		if (_grassWithinRange != null) {
			if (_grassWithinRange.Count > 0) {
				return true;
			}
		}
		return false;
	}

	bool DecideToWander()
	{
		if (_tilesWithinRange != null) {
			if (_tilesWithinRange.Count > 0) {
				return true;
			}
		}
		return false;
	}

	bool DecideToEvade()
	{
		if (_wolvesWithinRange != null) {
			if (_wolvesWithinRange.Count != 0) {
				return true;
			}
		}
		return false;
	}
		
	private void DecideGrassToSeek()
	{
		List<Script_Tile> tilesWithinRangeExcludingPosition = _grassWithinRange;

		Script_Tile tileToRemove = null;

		foreach(Script_Tile tile in _grassWithinRange)
		{
			if (GetCurrentGridPosition () == tile.GetPositionAsInt ())
				tileToRemove = tile;
		}
		if (tileToRemove != null)
			tilesWithinRangeExcludingPosition.Remove (tileToRemove);


		List<Script_Tile> availableGrass = new List<Script_Tile>();
		foreach (Script_Tile tile in tilesWithinRangeExcludingPosition) {
			if (tile.GetOccupiedBySheep () == false) {
				availableGrass.Add (tile);
			}
		}

		if (availableGrass.Count > 0) {
			
			_targetLocation = GetNearestGrass (availableGrass).GetPositionAsInt ();
			OccupyNewLocation (_targetLocation);
		}
		else {
			_targetLocation = GetCurrentGridPosition ();
			OccupyNewLocation (_targetLocation);
		}

	}
	private Script_Tile GetNearestGrass(List<Script_Tile> p_grassList)
	{
		List<Script_Tile> nearestGrassWithinRange = new List<Script_Tile> ();
		if (p_grassList.Count > 0) {
			int[] distanceArray = new int[p_grassList.Count];


			for (int i = 0; i < p_grassList.Count; i++) {
				distanceArray [i] = (int)Vector3Int.Distance (GetCurrentGridPosition (), p_grassList [i].GetPositionAsInt ());		
			}

			int shortestDistance = int.MaxValue;
			for (int j = 0; j < p_grassList.Count; j++) {
				if (shortestDistance > distanceArray [j]) {
					nearestGrassWithinRange.Clear ();
					shortestDistance = distanceArray [j];
				}

				if (shortestDistance == distanceArray [j]) {
					nearestGrassWithinRange.Add (p_grassList [j]);
				}

			}

			Script_Tile randomCloseGrass = null;
			if (nearestGrassWithinRange != null) {
				int randomNumber = Random.Range (0, nearestGrassWithinRange.Count);
				randomCloseGrass = nearestGrassWithinRange [randomNumber];
			}

			if (randomCloseGrass != null)
				return randomCloseGrass;
		}
		return null;
	}
	private void DecideWanderingLocation()
	{
		List<Script_Tile> tilesWithinRangeExcludingPosition = _tilesWithinRange;
		Script_Tile tileToRemove = null;

		foreach(Script_Tile tile in _tilesWithinRange)
		{
			if (GetCurrentGridPosition () == tile.GetPositionAsInt ())
				tileToRemove = tile;
		}
		if (tileToRemove != null)
			tilesWithinRangeExcludingPosition.Remove (tileToRemove);

		List<Script_Tile> availableTiles = new List<Script_Tile>();
		foreach (Script_Tile tile in tilesWithinRangeExcludingPosition) {
			if (tile.GetOccupiedBySheep () == false) {
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

	private bool DecideToEatGrass()
	{
		int x = GetCurrentGridPosition().x;
		int z = GetCurrentGridPosition().z;
		if (_grassWithinRange != null) {
			foreach (Script_Tile grass in _grassWithinRange) {
				if (_grid.AccessGridTile (x, z) == grass && (grass.GetOccupiedBySheep () != true || _grid.AccessGridTile (_occupiedLocation.x, _occupiedLocation.z) == grass)) {
					Vector3Int occupyLocation = new Vector3Int (x, 0, z);
					OccupyNewLocation (occupyLocation);
					_grassBeingEaten = grass;
					return true;
				}
			}
		}

		return false;

	}

	private bool DecideToReproduce()
	{
		if (_surroundingTiles != null && _health >= _maxHealth) {

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
				if (tile.GetOccupiedBySheep () == false) {
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
			if (tile.GetOccupiedBySheep () == false) {
				availableTiles.Add (tile);
			}
		}
		int randomNumber = Random.Range(0,availableTiles.Count-1);
		if (availableTiles.Count > 0) {
			return availableTiles [randomNumber].GetPositionAsInt ();
		}
		return Vector3Int.zero;
	}

	private void DecideDirectionToEvade()
	{
		if (_wolvesWithinRange.Count > 0) {
		Vector3 directionToEvadeIn = Vector3.zero;
		int divisor = 0;

		foreach (Script_Wolf wolf in _wolvesWithinRange.ToList()) {
				if (wolf.GetWolfObject() != null) {
					Vector3 vectorFromWolfToSheep = GetCurrentGridPosition () - wolf.GetCurrentGridPosition ();
					vectorFromWolfToSheep = vectorFromWolfToSheep.normalized;

					directionToEvadeIn += vectorFromWolfToSheep;
					divisor++;
				}
		}

			
			directionToEvadeIn = directionToEvadeIn / divisor;
			directionToEvadeIn = directionToEvadeIn.normalized;	

			int x = GetCurrentGridPosition ().x;
			int z = GetCurrentGridPosition ().z;

			int directionToWalkInX = (int)(directionToEvadeIn).x;
			int directionToWalkInZ = (int)(directionToEvadeIn).z;

			int evasionSprintRange = 4;

			for (int i = 1; i <= evasionSprintRange; i++) {

				int locationX = (x + (directionToWalkInX * i));
				int locationZ = (z + (directionToWalkInZ * i));

				if (locationX >= 0 && locationX < _grid.GetWidth () && locationZ >= 0 && locationZ < _grid.GetHeight () ) { 										
					if (_grid.AccessGridTile ( locationX , locationZ).GetOccupiedBySheep () == false) {
						_targetLocation = new Vector3Int ( locationX, 0, locationZ );
						OccupyNewLocation (_targetLocation);
					}
				}
			}
		}

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
		_grassBeingEaten = null;
		if (DecideToDie ()) {
			_decidedAction = DecidedAction.dying;
			_actDelegate = new ActDelegate (Die);
		}

		if (DecideToEvade () && !DecideToDie ()) {
			if (AtTargetLocation () || _decidedAction != DecidedAction.Evading) {
				DecideDirectionToEvade ();
			}
			_decidedAction = DecidedAction.Evading;
			_actDelegate = new ActDelegate (EvadeToLocation);
		}
			
		else if (DecideToReproduce () && !DecideToDie() && !DecideToEvade()) {
			_reproductionLocation =  DecideReproductionTile ();

			Script_Tile currentlyOccupiedTile = _grid.AccessGridTile (_reproductionLocation.x, _reproductionLocation.z);
			currentlyOccupiedTile.SetOccupiedBySheep (false);
			_decidedAction = DecidedAction.Reproducing;
			_actDelegate = new ActDelegate (Reproduce);
		}
		else if (DecideToEatGrass() && !DecideToDie() && !DecideToReproduce() && !DecideToEvade()) {
			OccupyNewLocation(GetCurrentGridPosition ());
			_decidedAction = DecidedAction.Eating;
			_actDelegate = new ActDelegate (EatGrass);
		}
		else if (DecideToSeekGrass () && !DecideToEatGrass() && !DecideToReproduce() && !DecideToDie() && !DecideToEvade()) {
			if (AtTargetLocation () || _decidedAction != DecidedAction.Seeking) {
				DecideGrassToSeek ();
			}
			_decidedAction = DecidedAction.Seeking;
			_actDelegate = new ActDelegate (MoveToLocation);
		}
		else if (DecideToWander () && !DecideToSeekGrass() && !DecideToEatGrass() && !DecideToReproduce() && !DecideToDie() && !DecideToEvade()) {
			if (AtTargetLocation() || _decidedAction != DecidedAction.Wandering)
				DecideWanderingLocation ();			
			_decidedAction = DecidedAction.Wandering;
			_actDelegate = new ActDelegate (MoveToLocation);
		}
	}

	private void MoveToLocation()
	{
		_health -= _movementDecay;

		if (Vector3.Distance (_sheepObject.transform.position, _targetLocation) > _size) {
			_sheepObject.transform.position += (_targetLocation - _sheepObject.transform.position).normalized * _speed;
		} else
			_sheepObject.transform.position = _targetLocation;
	}

	private void EvadeToLocation()
	{
		_health -= 0.01f;

		if (Vector3.Distance (_sheepObject.transform.position, _targetLocation) > _size) {
			_sheepObject.transform.position += (_targetLocation - _sheepObject.transform.position).normalized * _evasionSpeed;
		} else
			_sheepObject.transform.position = _targetLocation;
	}

	public void Act()
	{
		if (_actDelegate != null)
			_actDelegate ();
	}

	public Script_Tile GetGrassBeingEaten()
	{
		return _grassBeingEaten;
	}

	private void EatGrass()
	{
		_sheepObject.transform.position += (_grassBeingEaten.GetPositionAsInt() - _sheepObject.transform.position).normalized * _speed;
		float EatingGrassHealthIncrease = 0.15f;
		float damage = 0.5f;

		if (_grassBeingEaten.GetState () == GrassStates.Grass) {

			_health += EatingGrassHealthIncrease;
			_health = Mathf.Clamp (_health, 0, _maxHealth);
			if (_grassBeingEaten != null)
				_grassBeingEaten.DecreaseHealth (damage);
		} else {
			_health -= _actionDecay;
		}
	}

	private void Reproduce()
	{			
		float reproductionDamage = _health * 0.5f;

		if (_health >= _maxHealth) {
			_health = reproductionDamage;
			_grid.InstantiateSheep(_reproductionLocation);
		
		} else {
			_health -= _actionDecay;
		}

	}

	private void Die()
	{
		Vector3Int position = GetCurrentGridPosition ();
		_grid.AccessGridTile (position.x, position.z).NourishGrass();

		Script_Tile currentlyOccupiedTile = _grid.AccessGridTile (_occupiedLocation.x, _occupiedLocation.z);
		currentlyOccupiedTile.SetOccupiedBySheep (false);

		_grid.DestroySheep (this, _material);
		_sheepObject = null;
	}

	public void DecreaseHealth(float p_amount)
	{
		_health -= p_amount;
	}

}